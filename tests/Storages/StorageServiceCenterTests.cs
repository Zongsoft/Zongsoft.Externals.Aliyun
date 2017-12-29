using System;
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
	public class StorageServiceCenterTests
	{
		[Xunit.Fact]
		public void Test()
		{
			Assert.Equal(ServiceCenterName.Beijing, StorageServiceCenter.External.Beijing.Name);
			Assert.Equal(ServiceCenterName.Qingdao, StorageServiceCenter.External.Qingdao.Name);
			Assert.Equal(ServiceCenterName.Hangzhou, StorageServiceCenter.External.Hangzhou.Name);
			Assert.Equal(ServiceCenterName.Shenzhen, StorageServiceCenter.External.Shenzhen.Name);
			Assert.Equal(ServiceCenterName.Hongkong, StorageServiceCenter.External.Hongkong.Name);

			Assert.Equal(ServiceCenterName.Beijing, StorageServiceCenter.Internal.Beijing.Name);
			Assert.Equal(ServiceCenterName.Qingdao, StorageServiceCenter.Internal.Qingdao.Name);
			Assert.Equal(ServiceCenterName.Hangzhou, StorageServiceCenter.Internal.Hangzhou.Name);
			Assert.Equal(ServiceCenterName.Shenzhen, StorageServiceCenter.Internal.Shenzhen.Name);
			Assert.Equal(ServiceCenterName.Hongkong, StorageServiceCenter.Internal.Hongkong.Name);

			Assert.Equal(@"oss-cn-beijing.aliyuncs.com", StorageServiceCenter.External.Beijing.Path);
			Assert.Equal(@"oss-cn-qingdao.aliyuncs.com", StorageServiceCenter.External.Qingdao.Path);
			Assert.Equal(@"oss-cn-hangzhou.aliyuncs.com", StorageServiceCenter.External.Hangzhou.Path);
			Assert.Equal(@"oss-cn-shenzhen.aliyuncs.com", StorageServiceCenter.External.Shenzhen.Path);
			Assert.Equal(@"oss-cn-hongkong.aliyuncs.com", StorageServiceCenter.External.Hongkong.Path);

			Assert.Equal(@"oss-cn-beijing-internal.aliyuncs.com", StorageServiceCenter.Internal.Beijing.Path);
			Assert.Equal(@"oss-cn-qingdao-internal.aliyuncs.com", StorageServiceCenter.Internal.Qingdao.Path);
			Assert.Equal(@"oss-cn-hangzhou-internal.aliyuncs.com", StorageServiceCenter.Internal.Hangzhou.Path);
			Assert.Equal(@"oss-cn-shenzhen-internal.aliyuncs.com", StorageServiceCenter.Internal.Shenzhen.Path);
			Assert.Equal(@"oss-cn-hongkong-internal.aliyuncs.com", StorageServiceCenter.Internal.Hongkong.Path);
		}

		[Xunit.Fact]
		public void GetRequestUrlTest()
		{
			string resourcePath;

			Assert.Equal(@"http://automao-images.oss-cn-beijing.aliyuncs.com/automao-logo.png", StorageServiceCenter.External.Beijing.GetRequestUrl(@"/automao-images/automao-logo.png", out resourcePath));
			Assert.Equal(@"automao-logo.png", resourcePath);

			Assert.Equal(@"http://automao-images.oss-cn-beijing.aliyuncs.com/SaaS/", StorageServiceCenter.External.Beijing.GetRequestUrl(@"/automao-images/SaaS/", out resourcePath));
			Assert.Equal(@"SaaS/", resourcePath);

			Assert.Equal(@"http://automao-images.oss-cn-beijing.aliyuncs.com/SaaS", StorageServiceCenter.External.Beijing.GetRequestUrl(@"/automao-images/SaaS", out resourcePath));
			Assert.Equal(@"SaaS", resourcePath);
		}
	}
}
