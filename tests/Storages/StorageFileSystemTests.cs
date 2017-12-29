using System;
using System.IO;
using System.Collections.Generic;

using Zongsoft.IO;
using Zongsoft.Externals.Aliyun;
using Zongsoft.Externals.Aliyun.Storages;

using Xunit;

namespace Zongsoft.Externals.Aliyun.Tests.Storages
{
	public class StorageFileSystemTests
	{
		#region 常量定义
		private const string BUCKET_PATH = @"zfs.oss:/automao-images";
		private const string EXISTS_DIRECTORY_PATH = BUCKET_PATH + "/SaaS/";
		private const string EXISTS_FILE_PATH = BUCKET_PATH + "/automao-logo.png";
		private const string NOTEXISTS_DIRECTORY_PATH = BUCKET_PATH + "/NotExists-Directory/";
		private const string NOTEXISTS_FILE_PATH = BUCKET_PATH + "/NotExists-File";
		#endregion

		#region 构造函数
		public StorageFileSystemTests()
		{
			var configuration = Zongsoft.Options.Configuration.OptionConfiguration.Load(@"\Zongsoft\Zongsoft.Externals.Aliyun\src\Zongsoft.Externals.Aliyun.option");
			var option = configuration.GetOptionValue("Externals/Aliyun/OSS") as Zongsoft.Externals.Aliyun.Storages.Options.IConfiguration;

			var fileSystem = new StorageFileSystem(option);
			Zongsoft.IO.FileSystem.Providers.Register(fileSystem, typeof(Zongsoft.IO.IFileSystem));
		}
		#endregion

		#region 测试方法
		[Xunit.Fact]
		public void GetUrlTest()
		{
			var url = FileSystem.GetUrl(EXISTS_FILE_PATH);

			Assert.NotNull(url);
		}

		[Xunit.Fact]
		public void DeleteTest()
		{
			Assert.False(FileSystem.Directory.Delete(NOTEXISTS_DIRECTORY_PATH));
			Assert.False(FileSystem.File.Delete(NOTEXISTS_FILE_PATH));

			//Assert.True(FileSystem.File.Delete(@"zfs.oss:/automao-images/new-dir/test.jpg"));
		}

		[Xunit.Fact]
		public void ExistsTest()
		{
			Assert.True(FileSystem.Directory.Exists(EXISTS_DIRECTORY_PATH));
			Assert.True(FileSystem.File.Exists(EXISTS_FILE_PATH));

			Assert.False(FileSystem.Directory.Exists(NOTEXISTS_DIRECTORY_PATH));
			Assert.False(FileSystem.File.Exists(NOTEXISTS_FILE_PATH));

			var directoryPath = EXISTS_DIRECTORY_PATH.TrimEnd('/');
			Assert.True(directoryPath.Length > 0 && directoryPath[directoryPath.Length - 1] != '/');
			Assert.True(FileSystem.Directory.Exists(directoryPath));

			var filePath = EXISTS_FILE_PATH + "/";
			Assert.True(filePath.Length > 0 && filePath[filePath.Length - 1] == '/');
			Assert.True(FileSystem.File.Exists(filePath));
		}

		[Xunit.Fact]
		public void CreateDirectoryTest()
		{
			Assert.True(FileSystem.Directory.Create(NOTEXISTS_DIRECTORY_PATH));

			var info = FileSystem.Directory.GetInfo(NOTEXISTS_DIRECTORY_PATH);

			Assert.NotNull(info);
			Assert.True(FileSystem.Directory.Delete(NOTEXISTS_DIRECTORY_PATH));
		}

		[Xunit.Fact]
		public void OpenFileTest()
		{
			using(var stream = FileSystem.File.Open(EXISTS_FILE_PATH))
			{
				Assert.NotNull(stream);
				Assert.True(stream.CanRead);

				using(var ms = new MemoryStream())
				{
					stream.CopyTo(ms);

					Assert.True(ms.Length > 0);
				}
			}
		}
		#endregion
	}
}
