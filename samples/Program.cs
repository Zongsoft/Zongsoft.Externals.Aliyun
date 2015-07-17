using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zongsoft.Externals.Aliyun.Samples
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			int index = 1;

			while(true)
			{
				Console.WriteLine("Please input a file path to upload(press Enter to exit): ");

				Console.ForegroundColor = ConsoleColor.DarkYellow;
				var filePath = Console.ReadLine();
				Console.ResetColor();

				if(string.IsNullOrWhiteSpace(filePath))
					break;

				if(!File.Exists(filePath))
				{
					WriteLine(ConsoleColor.DarkRed, "FAILED: This '{0}' is not exists.", filePath);
					Console.WriteLine();

					continue;
				}

				using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					var path = string.Format("zfs.oss:/automao-data/samples/data-{0}{1}", index, Path.GetExtension(filePath));

					//执行文件上传
					StorageFileSample.Upload(path, stream);
				}

				WriteLine(ConsoleColor.Green, "[{0}] The file upload succeed!", index++);
				Console.WriteLine();
			}
		}

		private static void WriteLine(ConsoleColor color, string format, params object[] args)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(format, args);
			Console.ResetColor();
		}
	}
}
