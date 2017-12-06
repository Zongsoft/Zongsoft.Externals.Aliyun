/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Externals.Aliyun.
 *
 * Zongsoft.Externals.Aliyun is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Externals.Aliyun is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Externals.Aliyun; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Zongsoft.IO;
using Zongsoft.Services;

namespace Zongsoft.Externals.Aliyun.Storages
{
	[Zongsoft.Services.Matcher(typeof(Zongsoft.IO.FileSystem.Matcher))]
	public class StorageFileSystem : Zongsoft.IO.IFileSystem
	{
		#region 成员字段
		private Options.IConfiguration _configuration;
		private StorageFileProvider _file;
		private StorageDirectoryProvider _directory;
		#endregion

		#region 构造函数
		public StorageFileSystem()
		{
		}

		public StorageFileSystem(Options.IConfiguration configuration)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			_configuration = configuration;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取文件目录系统的方案，始终返回“zfs.oss”。
		/// </summary>
		public string Scheme
		{
			get
			{
				return "zfs.oss";
			}
		}

		public IFile File
		{
			get
			{
				if(_file == null)
					System.Threading.Interlocked.CompareExchange(ref _file, new StorageFileProvider(this), null);

				return _file;
			}
		}

		public IDirectory Directory
		{
			get
			{
				if(_directory == null)
					System.Threading.Interlocked.CompareExchange(ref _directory, new StorageDirectoryProvider(this), null);

				return _directory;
			}
		}

		public Options.IConfiguration Configuration
		{
			get
			{
				return _configuration;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_configuration = value;
			}
		}

		public Storages.StorageClient Client
		{
			get
			{
				var option = _configuration;

				if(option == null)
					return null;

				var client = Storages.StorageServiceCenter.GetInstance(option.Name, option.IsInternal).Client;
				client.Certification = option.Certification;
				return client;
			}
		}
		#endregion

		#region 公共方法
		public string GetUrl(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			var option = _configuration;

			if(option == null)
				throw new InvalidOperationException("The value of 'Option' property is null.");

			return StorageServiceCenter.GetInstance(option.Name, false).GetRequestUrl(path);
		}
		#endregion

		#region 嵌套子类
		private sealed class StorageDirectoryProvider : IDirectory
		{
			#region 成员字段
			private StorageFileSystem _fileSystem;
			private Storages.StorageClient _client;
			#endregion

			#region 私有构造
			internal StorageDirectoryProvider(StorageFileSystem fileSystem)
			{
				if(fileSystem == null)
					throw new ArgumentNullException("fileSystem");

				_fileSystem = fileSystem;
				_client = fileSystem.Client;
			}
			#endregion

			#region 公共方法
			public bool Create(string path, IDictionary<string, object> properties = null)
			{
				return Utility.ExecuteTask(() => _client.CreateAsync(this.EnsureDirectoryPath(path), properties));
			}

			public async Task<bool> CreateAsync(string path, IDictionary<string, object> properties = null)
			{
				return await _client.CreateAsync(this.EnsureDirectoryPath(path), properties);
			}

			public bool Delete(string path, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				return Utility.ExecuteTask(() => _client.DeleteAsync(this.EnsureDirectoryPath(path)));
			}

			public async Task<bool> DeleteAsync(string path, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				return await _client.DeleteAsync(this.EnsureDirectoryPath(path));
			}

			public void Move(string source, string destination)
			{
				source = this.EnsureDirectoryPath(source);
				destination = this.EnsureDirectoryPath(destination);

				if(Utility.ExecuteTask(() => _client.CopyAsync(source, destination)))
					Utility.ExecuteTask(() => _client.DeleteAsync(source));
			}

			public async Task MoveAsync(string source, string destination)
			{
				source = this.EnsureDirectoryPath(source);
				destination = this.EnsureDirectoryPath(destination);

				if(await _client.CopyAsync(source, destination))
					await _client.DeleteAsync(source);
			}

			public bool Exists(string path)
			{
				return Utility.ExecuteTask(() => _client.ExistsAsync(this.EnsureDirectoryPath(path)));
			}

			public async Task<bool> ExistsAsync(string path)
			{
				return await _client.ExistsAsync(this.EnsureDirectoryPath(path));
			}

			public Zongsoft.IO.DirectoryInfo GetInfo(string path)
			{
				path = this.EnsureDirectoryPath(path);
				var properties = Utility.ExecuteTask(() => _client.GetExtendedPropertiesAsync(path));

				return this.GenerateInfo(path, properties);
			}

			public async Task<Zongsoft.IO.DirectoryInfo> GetInfoAsync(string path)
			{
				path = this.EnsureDirectoryPath(path);
				var properties = await _client.GetExtendedPropertiesAsync(path);

				return this.GenerateInfo(path, properties);
			}

			public bool SetInfo(string path, IDictionary<string, object> properties)
			{
				return Utility.ExecuteTask(() => _client.SetExtendedPropertiesAsync(this.EnsureDirectoryPath(path), properties));
			}

			public async Task<bool> SetInfoAsync(string path, IDictionary<string, object> properties)
			{
				return await _client.SetExtendedPropertiesAsync(this.EnsureDirectoryPath(path), properties);
			}

			public IEnumerable<Zongsoft.IO.PathInfo> GetChildren(string path)
			{
				return this.GetChildren(path, null, false);
			}

			public IEnumerable<Zongsoft.IO.PathInfo> GetChildren(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				path = this.EnsurePatternPath(path, pattern);

				var list = Utility.ExecuteTask(() => _client.SearchAsync(path, p => _fileSystem.GetUrl(p)));

				if(list == null)
					return Enumerable.Empty<Zongsoft.IO.PathInfo>();

				return list;
			}

			public Task<IEnumerable<Zongsoft.IO.PathInfo>> GetChildrenAsync(string path)
			{
				return this.GetChildrenAsync(path, null, false);
			}

			public async Task<IEnumerable<Zongsoft.IO.PathInfo>> GetChildrenAsync(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				path = this.EnsurePatternPath(path, pattern);

				var list = await _client.SearchAsync(path, p => _fileSystem.GetUrl(p));

				if(list == null)
					return Enumerable.Empty<Zongsoft.IO.PathInfo>();

				return list;
			}

			public IEnumerable<Zongsoft.IO.DirectoryInfo> GetDirectories(string path)
			{
				return this.GetDirectories(path, null, false);
			}

			public IEnumerable<Zongsoft.IO.DirectoryInfo> GetDirectories(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				path = this.EnsurePatternPath(path, pattern);

				var list = Utility.ExecuteTask(() => _client.SearchAsync(path, p => _fileSystem.GetUrl(p)));

				if(list == null)
					return Enumerable.Empty<Zongsoft.IO.DirectoryInfo>();

				return list.Where(item => item.IsDirectory).Select(item => (Zongsoft.IO.DirectoryInfo)item);
			}

			public Task<IEnumerable<Zongsoft.IO.DirectoryInfo>> GetDirectoriesAsync(string path)
			{
				return this.GetDirectoriesAsync(path, null, false);
			}

			public async Task<IEnumerable<Zongsoft.IO.DirectoryInfo>> GetDirectoriesAsync(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				path = this.EnsurePatternPath(path, pattern);

				var list = await _client.SearchAsync(path, p => _fileSystem.GetUrl(p));

				if(list == null)
					return Enumerable.Empty<Zongsoft.IO.DirectoryInfo>();

				return list.Where(item => item.IsDirectory).Select(item => (Zongsoft.IO.DirectoryInfo)item);
			}

			public IEnumerable<Zongsoft.IO.FileInfo> GetFiles(string path)
			{
				return this.GetFiles(path, null, false);
			}

			public IEnumerable<Zongsoft.IO.FileInfo> GetFiles(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				path = this.EnsurePatternPath(path, pattern);

				var list = Utility.ExecuteTask(() => _client.SearchAsync(path, p => _fileSystem.GetUrl(p)));

				if(list == null)
					return Enumerable.Empty<Zongsoft.IO.FileInfo>();

				return list.Where(item => item.IsFile).Select(item => (Zongsoft.IO.FileInfo)item);
			}

			public Task<IEnumerable<Zongsoft.IO.FileInfo>> GetFilesAsync(string path)
			{
				return this.GetFilesAsync(path, null, false);
			}

			public async Task<IEnumerable<Zongsoft.IO.FileInfo>> GetFilesAsync(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				path = this.EnsurePatternPath(path, pattern);

				var list = await _client.SearchAsync(path, p => _fileSystem.GetUrl(p));

				if(list == null)
					return Enumerable.Empty<Zongsoft.IO.FileInfo>();

				return list.Where(item => item.IsFile).Select(item => (Zongsoft.IO.FileInfo)item);
			}
			#endregion

			#region 私有方法
			private string EnsureDirectoryPath(string path)
			{
				if(string.IsNullOrWhiteSpace(path))
					throw new ArgumentNullException("path");

				path = path.Trim();

				if(path.EndsWith("/"))
					return path;
				else
					return path + "/";
			}

			private string EnsurePatternPath(string path, string pattern)
			{
				if(string.IsNullOrWhiteSpace(pattern))
					return this.EnsureDirectoryPath(path);

				return this.EnsureDirectoryPath(path) + pattern.Trim('*', ' ', '\t', '\r', '\n').TrimStart('/');
			}

			private Zongsoft.IO.DirectoryInfo GenerateInfo(string path, IDictionary<string, object> properties)
			{
				if(properties == null)
					return null;

				DateTimeOffset createdTimeOffset, modifiedTimeOffset;
				DateTime? createdTime = null, modifiedTime = null;

				object value;

				if(properties.TryGetValue(StorageHeaders.ZFS_CREATEDTIME_PROPERTY, out value))
				{
					if(Zongsoft.Common.Convert.TryConvertValue(value, out createdTimeOffset))
						createdTime = createdTimeOffset.LocalDateTime;
				}

				if(properties.TryGetValue(StorageHeaders.HTTP_LAST_MODIFIED_PROPERTY, out value))
				{
					if(Zongsoft.Common.Convert.TryConvertValue(value, out modifiedTimeOffset))
						modifiedTime = modifiedTimeOffset.LocalDateTime;
				}

				var info = new Zongsoft.IO.DirectoryInfo(path, createdTime, modifiedTime, _fileSystem.GetUrl(path));

				foreach(var property in properties)
				{
					info.Properties[property.Key] = property.Value;
				}

				return info;
			}
			#endregion
		}

		private sealed class StorageFileProvider : IFile
		{
			#region 成员字段
			private StorageFileSystem _fileSystem;
			private Storages.StorageClient _client;
			#endregion

			#region 私有构造
			internal StorageFileProvider(StorageFileSystem fileSystem)
			{
				if(fileSystem == null)
					throw new ArgumentNullException("fileSystem");

				_fileSystem = fileSystem;
				_client = fileSystem.Client;
			}
			#endregion

			#region 公共方法
			public bool Delete(string path)
			{
				return Utility.ExecuteTask(() => _client.DeleteAsync(this.EnsureFilePath(path)));
			}

			public Task<bool> DeleteAsync(string path)
			{
				return _client.DeleteAsync(this.EnsureFilePath(path));
			}

			public bool Exists(string path)
			{
				return Utility.ExecuteTask(() => _client.ExistsAsync(this.EnsureFilePath(path)));
			}

			public Task<bool> ExistsAsync(string path)
			{
				return _client.ExistsAsync(this.EnsureFilePath(path));
			}

			public void Copy(string source, string destination)
			{
				this.Copy(source, destination, true);
			}

			public void Copy(string source, string destination, bool overwrite)
			{
				source = this.EnsureFilePath(source);
				destination = this.EnsureFilePath(destination);

				if(!overwrite)
				{
					if(Utility.ExecuteTask(() => _client.ExistsAsync(destination)))
						return;
				}

				Utility.ExecuteTask(() => _client.CopyAsync(source, destination));
			}

			public Task CopyAsync(string source, string destination)
			{
				return this.CopyAsync(source, destination, true);
			}

			public async Task CopyAsync(string source, string destination, bool overwrite)
			{
				source = this.EnsureFilePath(source);
				destination = this.EnsureFilePath(destination);

				if(!overwrite)
				{
					if(await _client.ExistsAsync(destination))
						return;
				}

				await _client.CopyAsync(source, destination);
			}

			public void Move(string source, string destination)
			{
				source = this.EnsureFilePath(source);
				destination = this.EnsureFilePath(destination);

				if(Utility.ExecuteTask(() => _client.CopyAsync(source, destination)))
					Utility.ExecuteTask(() => _client.DeleteAsync(source));
			}

			public async Task MoveAsync(string source, string destination)
			{
				source = this.EnsureFilePath(source);
				destination = this.EnsureFilePath(destination);

				if(await _client.CopyAsync(source, destination))
					await _client.DeleteAsync(source);
			}

			public Zongsoft.IO.FileInfo GetInfo(string path)
			{
				path = this.EnsureFilePath(path);
				var properties = Utility.ExecuteTask(() => _client.GetExtendedPropertiesAsync(path));

				return this.GenerateInfo(path, properties);
			}

			public async Task<Zongsoft.IO.FileInfo> GetInfoAsync(string path)
			{
				path = this.EnsureFilePath(path);
				var properties = await _client.GetExtendedPropertiesAsync(path);

				return this.GenerateInfo(path, properties);
			}

			public bool SetInfo(string path, IDictionary<string, object> properties)
			{
				return Utility.ExecuteTask(() => _client.SetExtendedPropertiesAsync(this.EnsureFilePath(path), properties));
			}

			public async Task<bool> SetInfoAsync(string path, IDictionary<string, object> properties)
			{
				return await _client.SetExtendedPropertiesAsync(this.EnsureFilePath(path), properties);
			}

			public Stream Open(string path, IDictionary<string, object> properties = null)
			{
				return this.Open(path, FileMode.Open, FileAccess.Read, FileShare.None, properties);
			}

			public Stream Open(string path, FileMode mode, IDictionary<string, object> properties = null)
			{
				return this.Open(path, mode, FileAccess.Read, FileShare.None, properties);
			}

			public Stream Open(string path, FileMode mode, FileAccess access, IDictionary<string, object> properties = null)
			{
				return this.Open(path, mode, access, FileShare.None, properties);
			}

			public Stream Open(string path, FileMode mode, FileAccess access, FileShare share, IDictionary<string, object> properties = null)
			{
				bool writable = (mode != FileMode.Open) || (access & FileAccess.Write) == FileAccess.Write;

				if(writable)
					return new StorageFileStream(_client.GetUploader(path, properties));

				return Utility.ExecuteTask(() => _client.DownloadAsync(this.EnsureFilePath(path), properties));
			}
			#endregion

			#region 私有方法
			private string EnsureFilePath(string path)
			{
				if(string.IsNullOrWhiteSpace(path))
					throw new ArgumentNullException("path");

				return path.Trim().TrimEnd('/', '\\');
			}

			private Zongsoft.IO.FileInfo GenerateInfo(string path, IDictionary<string, object> properties)
			{
				if(properties == null)
					return null;

				DateTimeOffset createdTimeOffset, modifiedTimeOffset;
				DateTime? createdTime = null, modifiedTime = null;
				int size = 0;
				byte[] checksum = null;
				object value;

				if(properties.TryGetValue(StorageHeaders.ZFS_CREATEDTIME_PROPERTY, out value))
				{
					if(Zongsoft.Common.Convert.TryConvertValue(value, out createdTimeOffset))
						createdTime = createdTimeOffset.LocalDateTime;
				}

				if(properties.TryGetValue(StorageHeaders.HTTP_LAST_MODIFIED_PROPERTY, out value))
				{
					if(Zongsoft.Common.Convert.TryConvertValue(value, out modifiedTimeOffset))
						modifiedTime = modifiedTimeOffset.LocalDateTime;
				}

				if(properties.TryGetValue(StorageHeaders.HTTP_CONTENT_LENGTH_PROPERTY, out value))
					Zongsoft.Common.Convert.TryConvertValue(value, out size);

				if(properties.TryGetValue(StorageHeaders.HTTP_ETAG_PROPERTY, out value) && value != null)
				{
					checksum = Zongsoft.Common.Convert.FromHexString(value.ToString().Trim('"'), '-');
				}

				var info = new Zongsoft.IO.FileInfo(path, size, checksum, createdTime, modifiedTime, _fileSystem.GetUrl(path));

				foreach(var property in properties)
				{
					info.Properties[property.Key] = property.Value;
				}

				return info;
			}
			#endregion

			#region 嵌套子类
			private class StorageFileStream : Stream
			{
				#region 成员字段
				private StorageUploader _uploader;
				private long _length;
				#endregion

				#region 构造函数
				internal StorageFileStream(StorageUploader uploader)
				{
					if(uploader == null)
						throw new ArgumentNullException("uploader");

					_uploader = uploader;
				}
				#endregion

				#region 公共属性
				public override bool CanRead
				{
					get
					{
						return false;
					}
				}

				public override bool CanSeek
				{
					get
					{
						return false;
					}
				}

				public override bool CanWrite
				{
					get
					{
						return true;
					}
				}

				public override long Length
				{
					get
					{
						var uploader = _uploader;

						if(uploader != null)
							return uploader.Length;

						return _length;
					}
				}

				public override long Position
				{
					get
					{
						//确认当前流是否可用
						var uploader = this.EnsureUploader();

						return uploader.Position;
					}
					set
					{
						throw new NotSupportedException();
					}
				}
				#endregion

				#region 公共方法
				public override void Flush()
				{
					//确认当前流是否可用
					var uploader = this.EnsureUploader();

					uploader.Flush();
				}

				public override int Read(byte[] buffer, int offset, int count)
				{
					throw new NotSupportedException();
				}

				public override long Seek(long offset, SeekOrigin origin)
				{
					throw new NotSupportedException();
				}

				public override void SetLength(long value)
				{
					throw new NotSupportedException();
				}

				public override void Write(byte[] buffer, int offset, int count)
				{
					//确认当前流是否可用
					var uploader = this.EnsureUploader();

					uploader.Write(buffer, offset, count);
				}
				#endregion

				#region 释放资源
				protected override void Dispose(bool disposing)
				{
					var uploader = System.Threading.Interlocked.Exchange(ref _uploader, null);

					if(uploader != null)
					{
						_length = uploader.Length;
						uploader.Dispose();
					}
				}
				#endregion

				#region 私有方法
				private StorageUploader EnsureUploader()
				{
					var uploader = _uploader;

					if(uploader == null)
						throw new ObjectDisposedException(typeof(StorageFileStream).FullName);

					return uploader;
				}
				#endregion
			}
			#endregion
		}
		#endregion
	}
}
