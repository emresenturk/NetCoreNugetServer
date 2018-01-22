using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NugetServer.Models;

namespace NugetServer.Controllers
{
	[Produces("application/json")]
	public class DatabaseController : Controller
	{
		ApplicationDataContext db;
		IConfiguration configuration;

		public DatabaseController(ApplicationDataContext db, IConfiguration configuration)
		{
			this.db = db;
			this.configuration = configuration;
		}

		[Route("BuildIndex")]
		public IndexResult BuildIndex()
		{
			var specs = RetrieveSpecs();

			// todo: semantic versioning & version comparison (write that version class!)
			// todo: use semantic versioning to compare different versions of same package, and set islatest... properties.

			var added = new List<Nuspec>();
			var updated = new List<Nuspec>();
			foreach (var spec in specs)
			{
				var package = db.Packages.FirstOrDefault(p => p.Identifier == spec.Metadata.Id && p.Version == spec.Metadata.Version);
				if (package == null)
				{
					added.Add(spec);
					AddNewPackage(spec);
				}
				else
				{
					updated.Add(spec);
					UpdatePackage(package, spec);
				}
			}

			var remainingKeys = added.Union(updated).Select(spec => $"{spec.Metadata.Id}:{spec.Metadata.Version}").ToArray();

			var deletedPackages = db.Packages.Select(p => new { Package = p, Key = $"{p.Identifier}:{p.Version}" }).Where(p => !remainingKeys.Contains(p.Key)).Select(p => p.Package).ToList();

			var deleted = deletedPackages.Select(dp => $"{dp.Identifier}:{dp.Version}").ToArray();
			db.RemoveRange(deletedPackages);

			db.SaveChanges();
			return new IndexResult
			{
				Added = added.Select(p => $"{p.Metadata.Id}:{p.Metadata.Version}").ToArray(),
				Updated = updated.Select(p => $"{p.Metadata.Id}:{p.Metadata.Version}").ToArray(),
				Deleted = deleted
			};
		}

		private void UpdatePackage(Package package, Nuspec spec)
		{
			var baseUrl = $"{Request.Scheme.ToLower()}://{Request.Host}";

			package.Identifier = spec.Metadata.Id;
			package.Description = spec.Metadata.Description;
			package.Dependencies = CreateDependencyString(spec.Metadata.DependencySets);
			package.MinClientVersion = spec.Metadata.MinClientVersion;
			package.Version = spec.Metadata.Version;
			package.IsPrerelease = spec.Metadata.Version.Contains("beta");
			package.Title = spec.Metadata.Title ?? spec.Metadata.Id;
			package.Authors = spec.Metadata.Authors;
			package.Owners = spec.Metadata.Owners;
			package.IconUrl = spec.Metadata.IconUrl;
			package.LicenseUrl = spec.Metadata.LicenseUrl;
			package.ProjectUrl = spec.Metadata.ProjectUrl;
			package.RequireLicenseAcceptance = spec.Metadata.RequireLicenseAcceptance;
			package.DevelopmentDependency = spec.Metadata.DevelopmentDependency;
			package.Summary = spec.Metadata.Summary;
			package.ReleaseNotes = spec.Metadata.ReleaseNotes;
			package.Tags = spec.Metadata.Tags;
			package.PackageSize = spec.Size;
			package.PackageHash = spec.Hash;
			package.PackageHashAlgorithm = "SHA512";
			package.GalleryDetailsUrl = $"{baseUrl}/Package/{spec.Metadata.Id}/{spec.Metadata.Version}";
			package.TargetFrameworks = spec.TargetFrameworks;
		}

		private void AddNewPackage(Nuspec spec)
		{
			var baseUrl = $"{Request.Scheme.ToLower()}://{Request.Host}";

			db.Packages.Add(new Package
			{
				Identifier = spec.Metadata.Id,
				Description = spec.Metadata.Description,
				Dependencies = CreateDependencyString(spec.Metadata.DependencySets),
				MinClientVersion = spec.Metadata.MinClientVersion,
				Version = spec.Metadata.Version,
				IsPrerelease = spec.Metadata.Version.Contains("beta"),
				Title = spec.Metadata.Title ?? spec.Metadata.Id,
				Authors = spec.Metadata.Authors,
				Owners = spec.Metadata.Owners,
				IconUrl = spec.Metadata.IconUrl,
				LicenseUrl = spec.Metadata.LicenseUrl,
				ProjectUrl = spec.Metadata.ProjectUrl,
				RequireLicenseAcceptance = spec.Metadata.RequireLicenseAcceptance,
				DevelopmentDependency = spec.Metadata.DevelopmentDependency,
				Summary = spec.Metadata.Summary,
				ReleaseNotes = spec.Metadata.ReleaseNotes,
				Tags = spec.Metadata.Tags,
				PackageSize = spec.Size,
				PackageHash = spec.Hash,
				PackageHashAlgorithm = "SHA512",
				GalleryDetailsUrl = $"{baseUrl}/Package/{spec.Metadata.Id}/{spec.Metadata.Version}",
				TargetFrameworks = spec.TargetFrameworks
			});
		}

		private List<Nuspec> RetrieveSpecs()
		{
			var packageFileNames = GetPackageFileNames();
			var serializer = CreateSerializer();
			var hashAlgorithm = SHA512.Create();
			var specs = new List<Nuspec>();
			foreach (var packageFileName in packageFileNames)
			{
				long fileSize;
				byte[] fileHashBytes;
				using (var archiveFileStream = System.IO.File.Open(packageFileName, FileMode.Open))
				{
					fileSize = archiveFileStream.Length;
					fileHashBytes = hashAlgorithm.ComputeHash(archiveFileStream);
				}

				using (var archive = ZipFile.Open(packageFileName, ZipArchiveMode.Read))
				{
					var targetFrameworks = string.Join(",", archive.Entries.Where(e => e.FullName.Contains("lib/")).SelectMany(e => GetCompatibleFrameworkNames(e.FullName.Split('/')[1])));
					var nuspecFile = archive.Entries.First(e => e.Name.EndsWith(".nuspec", StringComparison.Ordinal));
					using (var stream = nuspecFile.Open())
					{
						if (serializer.Deserialize(stream) is Nuspec spec)
						{
							spec.Size = fileSize;
							spec.Hash = Convert.ToBase64String(fileHashBytes);
							spec.TargetFrameworks = targetFrameworks;
							specs.Add(spec);
						}
					}
				}
			}

			return specs;
		}

		string[] GetCompatibleFrameworkNames(string version)
		{
			switch (version)
			{
				case string s when s.Contains("netstandard"):
					return NetStandardCompatibleMonikers(version);
				default:
					return new[] { version };
			}
		}

		string[] NetStandardCompatibleMonikers(string netStandardMoniker)
		{
			switch (netStandardMoniker)
			{
				case "netstandard1.6":
					return new[] { "netstandard1.6", "netstandard2.0", "net461", "netcoreapp1.0", "netcoreapp1.1" };
				case "netstandard2.0":
					return new[] { "netstandard2.0","net461", "netcoreapp2.0" };
				default:
					return new string[] { };
			}
		}

		private IEnumerable<string> GetPackageFileNames()
		{
			return Directory.EnumerateFiles(configuration.GetValue<string>("Package_Directory"), "*.nupkg");
		}

		private static XmlSerializer CreateSerializer()
		{
			return new XmlSerializer(typeof(Nuspec));
		}

		private static string FrameworkNameToMoniker(string frameworkName)
		{
			const string netframeworkFrameworkName = ".NETFramework";
			const string netframeworkMonikerPrefix = "net";
			const string netcoreAppFrameworkName = ".NETCoreApp";
			const string netcoreAppFrameworkMoniker = "netcoreapp";
			const string netstandardFrameworkName = ".NETStandard";
			const string netstandardFrameworkMoniker = "netstandard";
			if (frameworkName.Contains(netframeworkFrameworkName))
			{
				var versionPart = frameworkName.Replace(netframeworkFrameworkName, string.Empty);
				return $"{netframeworkMonikerPrefix}{versionPart.Replace(".", string.Empty)}";
			}

			if (frameworkName.Contains(netcoreAppFrameworkName))
			{
				var versionPart = frameworkName.Replace(netcoreAppFrameworkName, string.Empty);
				return $"{netcoreAppFrameworkMoniker}{versionPart}";
			}

			if (frameworkName.Contains(netstandardFrameworkName))
			{
				var versionPart = frameworkName.Replace(netstandardFrameworkName, string.Empty);
				return $"{netstandardFrameworkMoniker}{versionPart}";
			}

			else
			{
				throw new ArgumentOutOfRangeException(nameof(frameworkName));
			}
		}

		private static string CreateDependencyString(IEnumerable<NuspecDependencySet> sets)
		{
			return string.Join("|", sets.Select(CreateDependencyString));
		}

		private static string CreateDependencyString(NuspecDependencySet set)
		{
			if (set.Dependencies == null || set.Dependencies.Count == 0)
			{
				if (set.TargetFramework != null)
				{
					return $"::{FrameworkNameToMoniker(set.TargetFramework)}";
				}
			}

			if (set.TargetFramework == null)
			{
				return string.Join("|", set.Dependencies.Select(d => string.IsNullOrEmpty(d.Version) ? d.Id : $"{d.Id}:[{d.Version}, )"));
			}

			return string.Join("|", set.Dependencies.Select(d => $"{d.Id}:[{d.Version}, ):{FrameworkNameToMoniker(set.TargetFramework)}"));
		}
	}

	public class IndexResult
	{
		public int UpdatedCount { get { return Updated.Length; } }

		public string[] Updated { get; set; }

		public int AddedCount { get { return Added.Length; } }

		public string[] Added { get; set; }

		public int DeletedCount { get { return Deleted.Length; } }

		public string[] Deleted { get; set; }
	}

	[XmlType("package")]
	[XmlRoot(Namespace = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd", ElementName = "package")]
	public class Nuspec
	{
		[XmlElement("metadata", IsNullable = false)]
		public NuspecMetadata Metadata { get; set; }

		[XmlArray("files")]
		public List<NuspecFile> Files { get; set; }

		[XmlIgnore]
		public long Size { get; set; }

		[XmlIgnore]
		public string Hash { get; set; }

		[XmlIgnore]
		public string TargetFrameworks { get; set; }
	}

	[XmlType("metadata")]
	public class NuspecMetadata
	{
		[XmlAttribute("minClientVersion")]
		public string MinClientVersion { get; set; }

		[XmlElement("id")]
		public string Id { get; set; }

		[XmlElement("version")]
		public string Version { get; set; }

		[XmlElement("title")]
		public string Title { get; set; }

		[XmlElement("authors")]
		public string Authors { get; set; }

		[XmlElement("owners")]
		public string Owners { get; set; }

		[XmlElement("licenseUrl")]
		public string LicenseUrl { get; set; }

		[XmlElement("projectUrl")]
		public string ProjectUrl { get; set; }

		[XmlElement("iconUrl")]
		public string IconUrl { get; set; }

		[XmlElement("requireLicenseAcceptance")]
		public bool RequireLicenseAcceptance { get; set; }

		[XmlElement("developmentDependency")]
		public string DevelopmentDependency { get; set; }

		[XmlElement("description")]
		public string Description { get; set; }

		[XmlElement("summary")]
		public string Summary { get; set; }

		[XmlElement("releaseNotes")]
		public string ReleaseNotes { get; set; }

		[XmlElement("copyright")]
		public string Copyright { get; set; }

		[XmlElement("language")]
		public string Language { get; set; }

		[XmlElement("tags")]
		public string Tags { get; set; }

		[XmlArray("dependencies")]
		[XmlArrayItem("group", typeof(NuspecDependencySet))]
		[XmlArrayItem("dependency", typeof(NuspecDependency))]
		public List<object> DependencySerialization { get; set; }

		[XmlIgnore]
		public IEnumerable<NuspecDependencySet> DependencySets
		{
			get
			{
				if (DependencySerialization[0] is NuspecDependencySet)
				{
					return DependencySerialization.Cast<NuspecDependencySet>();
				}
				else if (DependencySerialization[0] is NuspecDependency)
				{
					return new List<NuspecDependencySet> { new NuspecDependencySet { Dependencies = DependencySerialization.Cast<NuspecDependency>().ToList() } };
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
		}
	}

	[XmlType("file")]
	public class NuspecFile
	{
		[XmlAttribute("src")]
		public string Source { get; set; }

		[XmlAttribute("target")]
		public string Target { get; set; }

		[XmlAttribute("exclude")]
		public string Exclude { get; set; }
	}

	[XmlType("dependency")]
	public class NuspecDependency
	{
		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlAttribute("version")]
		public string Version { get; set; }

		[XmlAttribute("include")]
		public string Include { get; set; }

		[XmlAttribute("exclude")]
		public string Exclude { get; set; }
	}

	[XmlType("group")]
	public class NuspecDependencySet
	{
		[XmlAttribute("targetFramework")]
		public string TargetFramework { get; set; }

		[XmlElement("dependency")]
		public List<NuspecDependency> Dependencies { get; set; }
	}
}