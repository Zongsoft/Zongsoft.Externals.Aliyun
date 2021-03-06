﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Zongsoft.Externals.Aliyun;
using Zongsoft.Externals.Aliyun.Storages;

namespace Zongsoft.Externals.Aliyun.Tests.Storages
{
	public class StorageClientTests
	{
		#region 常量定义
		private const string BUCKET_NAME = "Bucket-Name";
		private const string EXISTS_DIRECTORY_PATH = "/" + BUCKET_NAME + "/SaaS/";
		private const string EXISTS_FILE_PATH = "/" + BUCKET_NAME + "/automao-logo.png";
		private const string NOTEXISTS_DIRECTORY_PATH = "/" + BUCKET_NAME + "/NotExists-Directory/";
		private const string NOTEXISTS_FILE_PATH = "/" + BUCKET_NAME + "/NotExists-File";
		#endregion

		#region 私有字段
		private Zongsoft.Externals.Aliyun.Storages.StorageClient _client;
		#endregion

		#region 构造函数
		public StorageClientTests()
		{
			var configuration = Zongsoft.Options.Configuration.OptionConfiguration.Load(@"\Zongsoft\Zongsoft.Externals.Aliyun\src\Zongsoft.Externals.Aliyun.option");
			var option = configuration.GetOptionValue("Externals/Aliyun/OSS") as Aliyun.Storages.Options.IConfiguration;

			//_client = StorageServiceCenter.GetInstance(option.Name, option.IsInternal).Client;
		}
		#endregion

		#region 测试方法
		[Xunit.Fact]
		public void CopyTest()
		{
			_client.CopyAsync($"/{BUCKET_NAME}/SaaS/toyota-logo.jpg", $"/{BUCKET_NAME}/SaaS/Toyota-Logo.jpg").Wait();
		}

		[Xunit.Fact]
		public void DeleteTest()
		{
			//Assert.True(_client.Delete(@"/automao-images/SaaS/test/test.jpg").Result);
			//Assert.True(_client.Delete(@"/automao-images/SaaS/test/").Result);

			Assert.False(_client.DeleteAsync(NOTEXISTS_DIRECTORY_PATH).Result);
			Assert.False(_client.DeleteAsync(NOTEXISTS_FILE_PATH).Result);
		}

		[Xunit.Fact]
		public void DownloadTest()
		{
			var stream = _client.DownloadAsync($"/{BUCKET_NAME}/automao-logo.png?ac=yes&response-content-encoding=utf8&acl=yes&bb=no").Result;

			Assert.NotNull(stream);
		}

		[Xunit.Fact]
		public void CreateTest()
		{
			_client.CreateAsync(NOTEXISTS_DIRECTORY_PATH).Wait();

			Assert.True(_client.DeleteAsync(NOTEXISTS_DIRECTORY_PATH).Result);
			Assert.False(_client.DeleteAsync(NOTEXISTS_DIRECTORY_PATH).Result);

			using(var stream = new FileStream(@"D:\temp\MM.jpg", FileMode.Open, FileAccess.Read))
			{
				_client.CreateAsync(NOTEXISTS_FILE_PATH, stream).Wait();

				Assert.True(_client.DeleteAsync(NOTEXISTS_FILE_PATH).Result);
				Assert.False(_client.DeleteAsync(NOTEXISTS_FILE_PATH).Result);
			}
		}

		[Xunit.Fact]
		public void ExistsTest()
		{
			Assert.True(_client.ExistsAsync(EXISTS_DIRECTORY_PATH).Result);
			Assert.True(_client.ExistsAsync(EXISTS_FILE_PATH).Result);

			Assert.False(_client.ExistsAsync(NOTEXISTS_DIRECTORY_PATH).Result);
			Assert.False(_client.ExistsAsync(NOTEXISTS_FILE_PATH).Result);
		}

		[Xunit.Fact]
		public void GetExtendedPropertiesTest()
		{
			var dictionary = _client.GetExtendedPropertiesAsync(EXISTS_DIRECTORY_PATH).Result;
			Assert.NotNull(dictionary);

			dictionary = _client.GetExtendedPropertiesAsync(EXISTS_FILE_PATH).Result;
			Assert.NotNull(dictionary);

			try
			{
				dictionary = _client.GetExtendedPropertiesAsync(NOTEXISTS_DIRECTORY_PATH).Result;
				Assert.Null(dictionary);
			}
			catch
			{
			}

			try
			{
				dictionary = _client.GetExtendedPropertiesAsync(NOTEXISTS_FILE_PATH).Result;
				Assert.Null(dictionary);
			}
			catch
			{
			}
		}

		[Xunit.Fact]
		public void SetExtendedPropertiesTest()
		{
			var dictionary = new Dictionary<string, object>()
			{
				{"WeChat-ID", "My WeChat"},
			};

			_client.SetExtendedPropertiesAsync(EXISTS_FILE_PATH, dictionary).Wait();
		}

		[Xunit.Fact]
		public void SearchTest()
		{
			var result = _client.SearchAsync(NOTEXISTS_FILE_PATH, null).Result;

			//var list = _client.Search(@"/automao-images/SaaS/*").Result;

			//Assert.NotNull(list);
			//Assert.Equal("automao-images", list.Name);
			//Assert.Equal("SaaS/", list.Pattern);

			//foreach(var item in list)
			//{
			//	item.ToString();
			//}
		}
		#endregion
	}
}
