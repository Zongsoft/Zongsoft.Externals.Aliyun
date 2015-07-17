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
			Assert.Equal(ServiceCenterName.Beijing, StorageServiceCenter.Public.Beijing.Name);
			Assert.Equal(ServiceCenterName.Qingdao, StorageServiceCenter.Public.Qingdao.Name);
			Assert.Equal(ServiceCenterName.Hangzhou, StorageServiceCenter.Public.Hangzhou.Name);
			Assert.Equal(ServiceCenterName.Shenzhen, StorageServiceCenter.Public.Shenzhen.Name);
			Assert.Equal(ServiceCenterName.Hongkong, StorageServiceCenter.Public.Hongkong.Name);

			Assert.Equal(ServiceCenterName.Beijing, StorageServiceCenter.Internal.Beijing.Name);
			Assert.Equal(ServiceCenterName.Qingdao, StorageServiceCenter.Internal.Qingdao.Name);
			Assert.Equal(ServiceCenterName.Hangzhou, StorageServiceCenter.Internal.Hangzhou.Name);
			Assert.Equal(ServiceCenterName.Shenzhen, StorageServiceCenter.Internal.Shenzhen.Name);
			Assert.Equal(ServiceCenterName.Hongkong, StorageServiceCenter.Internal.Hongkong.Name);

			Assert.Equal(@"oss-cn-beijing.aliyuncs.com", StorageServiceCenter.Public.Beijing.Path);
			Assert.Equal(@"oss-cn-qingdao.aliyuncs.com", StorageServiceCenter.Public.Qingdao.Path);
			Assert.Equal(@"oss-cn-hangzhou.aliyuncs.com", StorageServiceCenter.Public.Hangzhou.Path);
			Assert.Equal(@"oss-cn-shenzhen.aliyuncs.com", StorageServiceCenter.Public.Shenzhen.Path);
			Assert.Equal(@"oss-cn-hongkong.aliyuncs.com", StorageServiceCenter.Public.Hongkong.Path);

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

			Assert.Equal(@"http://automao-images.oss-cn-beijing.aliyuncs.com/automao-logo.png", StorageServiceCenter.Public.Beijing.GetRequestUrl(@"/automao-images/automao-logo.png", out resourcePath));
			Assert.Equal(@"automao-logo.png", resourcePath);

			Assert.Equal(@"http://automao-images.oss-cn-beijing.aliyuncs.com/SaaS/", StorageServiceCenter.Public.Beijing.GetRequestUrl(@"/automao-images/SaaS/", out resourcePath));
			Assert.Equal(@"SaaS/", resourcePath);

			Assert.Equal(@"http://automao-images.oss-cn-beijing.aliyuncs.com/SaaS", StorageServiceCenter.Public.Beijing.GetRequestUrl(@"/automao-images/SaaS", out resourcePath));
			Assert.Equal(@"SaaS", resourcePath);
		}
	}
}
