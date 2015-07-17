using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zongsoft.Externals.Aliyun.Samples
{
	public static class StorageFileSample
	{
		#region 静态构造
		static StorageFileSample()
		{
			var configuration = Zongsoft.Options.Configuration.OptionConfiguration.Load(@"\Zongsoft\Zongsoft.Externals.Aliyun\src\Zongsoft.Externals.Aliyun.option");
			var option = configuration.GetOptionObject("Externals/Aliyun/General") as Zongsoft.Externals.Aliyun.Options.Configuration.GeneralConfiguration;

			var fileSystem = new Zongsoft.Externals.Aliyun.Storages.StorageFileSystem(option);
			Zongsoft.IO.FileSystem.Providers.Register(fileSystem, typeof(Zongsoft.IO.IFileSystem));
		}
		#endregion

		#region 公共方法
		public static void Upload(string path, Stream stream)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			if(stream == null)
				throw new ArgumentNullException("stream");

			using(var destination = Zongsoft.IO.FileSystem.File.Open(path, FileMode.Create, FileAccess.Write))
			{
				stream.CopyTo(destination, 400 * 1024);
			}
		}
		#endregion
	}
}
