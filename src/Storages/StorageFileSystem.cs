﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2017 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Zongsoft.IO;

namespace Zongsoft.Externals.Aliyun.Storages
{
	[Zongsoft.Services.Matcher(typeof(Zongsoft.IO.FileSystem.Matcher))]
	public class StorageFileSystem : Zongsoft.IO.IFileSystem
	{
		#region 成员字段
		private Options.IConfiguration _configuration;
		private StorageFileProvider _file;
		private StorageDirectoryProvider _directory;
		private ConcurrentDictionary<string, StorageClient> _pool;
		#endregion

		#region 构造函数
		public StorageFileSystem()
		{
			_file = new StorageFileProvider(this);
			_directory = new StorageDirectoryProvider(this);
			_pool = new ConcurrentDictionary<string, StorageClient>();
		}

		public StorageFileSystem(Options.IConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			_file = new StorageFileProvider(this);
			_directory = new StorageDirectoryProvider(this);
			_pool = new ConcurrentDictionary<string, StorageClient>();
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
				return _file;
			}
		}

		public IDirectory Directory
		{
			get
			{
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
				_configuration = value ?? throw new ArgumentNullException();
			}
		}
		#endregion

		#region 公共方法
		public string GetUrl(string path)
		{
			if(string.IsNullOrEmpty(path))
				return null;

			return this.GetUrl(Zongsoft.IO.Path.Parse(path));
		}

		public string GetUrl(Zongsoft.IO.Path path)
		{
			if(path == null || path.Segments.Length == 0)
				return null;

			//确认OSS对象存储配置
			var configuration = this.EnsureConfiguration();

			//获取当前路径对应的存储器配置项，注：BucketName即为路径中的第一节
			var bucket = configuration.Buckets.Get(path.Segments[0], false);

			//获取当前路径对应的服务区域
			var region = this.GetRegion(bucket);

			return StorageServiceCenter.GetInstance(region, false).GetRequestUrl(path.FullPath);
		}
		#endregion

		#region 内部方法
		internal StorageClient GetClient(string bucketName)
		{
			if(string.IsNullOrEmpty(bucketName))
				throw new ArgumentNullException(nameof(bucketName));

			return _pool.GetOrAdd(bucketName, key =>
			{
				return this.CreateClient(bucketName);
			});
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
		private StorageClient CreateClient(string bucketName)
		{
			//确认OSS对象存储配置
			var configuration = this.EnsureConfiguration();

			//获取指定名称的存储器配置项
			var bucket = configuration.Buckets.Get(bucketName, false);

			var region = this.GetRegion(bucket);
			var center = StorageServiceCenter.GetInstance(region, Aliyun.Configuration.Instance.IsInternal);
			var certificate = this.GetCertificate(bucket);

			return new StorageClient(center, certificate);
		}
		#endregion

		#region 私有方法
		private ICertificate GetCertificate(Options.IBucketOption bucket)
		{
			var certificate = bucket?.Certificate;

			if(string.IsNullOrWhiteSpace(certificate))
				certificate = _configuration?.Certificate;

			if(string.IsNullOrWhiteSpace(certificate))
				return Aliyun.Configuration.Instance.Certificates.Default;

			return Aliyun.Configuration.Instance.Certificates.Get(certificate, true);
		}

		private ServiceCenterName GetRegion(Options.IBucketOption bucket)
		{
			return bucket?.Region ?? _configuration?.Region ?? Aliyun.Configuration.Instance.Name;
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private Options.IConfiguration EnsureConfiguration()
		{
			return this.Configuration ?? throw new InvalidOperationException("Missing required configuration of the OSS file-system(aliyun).");
		}
		#endregion

		#region 嵌套子类
		private sealed class StorageDirectoryProvider : IDirectory
		{
			#region 成员字段
			private StorageFileSystem _fileSystem;
			#endregion

			#region 私有构造
			internal StorageDirectoryProvider(StorageFileSystem fileSystem)
			{
				_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
			}
			#endregion

			#region 公共方法
			public bool Create(string path, IDictionary<string, object> properties = null)
			{
				return Utility.ExecuteTask(() => this.CreateAsync(path, properties));
			}

			public Task<bool> CreateAsync(string path, IDictionary<string, object> properties = null)
			{
				var directory = this.EnsureDirectoryPath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);
				return client.CreateAsync(directory, properties);
			}

			public bool Delete(string path, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				return Utility.ExecuteTask(() => this.DeleteAsync(path, recursive));
			}

			public Task<bool> DeleteAsync(string path, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				var directory = this.EnsureDirectoryPath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);
				return client.DeleteAsync(directory);
			}

			public void Move(string source, string destination)
			{
				throw new NotSupportedException();
			}

			public Task MoveAsync(string source, string destination)
			{
				throw new NotSupportedException();
			}

			public bool Exists(string path)
			{
				return Utility.ExecuteTask(() => this.ExistsAsync(path));
			}

			public Task<bool> ExistsAsync(string path)
			{
				var directory = this.EnsureDirectoryPath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);
				return client.ExistsAsync(directory);
			}

			public Zongsoft.IO.DirectoryInfo GetInfo(string path)
			{
				return Utility.ExecuteTask(() => this.GetInfoAsync(path));
			}

			public async Task<Zongsoft.IO.DirectoryInfo> GetInfoAsync(string path)
			{
				var directory = this.EnsureDirectoryPath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);
				var properties = await client.GetExtendedPropertiesAsync(directory);

				return this.GenerateInfo(path, properties);
			}

			public bool SetInfo(string path, IDictionary<string, object> properties)
			{
				return Utility.ExecuteTask(() => this.SetInfoAsync(path, properties));
			}

			public Task<bool> SetInfoAsync(string path, IDictionary<string, object> properties)
			{
				var directory = this.EnsureDirectoryPath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);
				return client.SetExtendedPropertiesAsync(directory, properties);
			}

			public IEnumerable<Zongsoft.IO.PathInfo> GetChildren(string path)
			{
				return this.GetChildren(path, null, false);
			}

			public IEnumerable<Zongsoft.IO.PathInfo> GetChildren(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				return Utility.ExecuteTask(() => this.GetChildrenAsync(path, pattern, recursive));
			}

			public Task<IEnumerable<Zongsoft.IO.PathInfo>> GetChildrenAsync(string path)
			{
				return this.GetChildrenAsync(path, null, false);
			}

			public async Task<IEnumerable<Zongsoft.IO.PathInfo>> GetChildrenAsync(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				var directory = this.EnsurePatternPath(path, pattern, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);

				var result = await client.SearchAsync(directory, p => _fileSystem.GetUrl(p));

				if(result == null)
					return Enumerable.Empty<Zongsoft.IO.PathInfo>();

				return result;
			}

			public IEnumerable<Zongsoft.IO.DirectoryInfo> GetDirectories(string path)
			{
				return this.GetDirectories(path, null, false);
			}

			public IEnumerable<Zongsoft.IO.DirectoryInfo> GetDirectories(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				return Utility.ExecuteTask(() => this.GetDirectoriesAsync(path, pattern, recursive));
			}

			public Task<IEnumerable<Zongsoft.IO.DirectoryInfo>> GetDirectoriesAsync(string path)
			{
				return this.GetDirectoriesAsync(path, null, false);
			}

			public async Task<IEnumerable<Zongsoft.IO.DirectoryInfo>> GetDirectoriesAsync(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				var directory = this.EnsurePatternPath(path, pattern, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);

				var result = await client.SearchAsync(directory, p => _fileSystem.GetUrl(p));

				if(result == null)
					return Enumerable.Empty<Zongsoft.IO.DirectoryInfo>();

				return result.Where(item => item.IsDirectory).Select(item => (Zongsoft.IO.DirectoryInfo)item);
			}

			public IEnumerable<Zongsoft.IO.FileInfo> GetFiles(string path)
			{
				return this.GetFiles(path, null, false);
			}

			public IEnumerable<Zongsoft.IO.FileInfo> GetFiles(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				return Utility.ExecuteTask(() => this.GetFilesAsync(path, pattern, recursive));
			}

			public Task<IEnumerable<Zongsoft.IO.FileInfo>> GetFilesAsync(string path)
			{
				return this.GetFilesAsync(path, null, false);
			}

			public async Task<IEnumerable<Zongsoft.IO.FileInfo>> GetFilesAsync(string path, string pattern, bool recursive = false)
			{
				if(recursive)
					throw new NotSupportedException();

				var directory = this.EnsurePatternPath(path, pattern, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);

				var result = await client.SearchAsync(directory, p => _fileSystem.GetUrl(p));

				if(result == null)
					return Enumerable.Empty<Zongsoft.IO.FileInfo>();

				return result.Where(item => item.IsFile).Select(item => (Zongsoft.IO.FileInfo)item);
			}
			#endregion

			#region 私有方法
			private string EnsureDirectoryPath(string text, out string bucketName)
			{
				var path = Zongsoft.IO.Path.Parse(text);

				if(!path.IsDirectory)
					throw new PathException("Invalid directory path.");

				if(path.Segments.Length == 0)
					throw new PathException("Missing bucket name of the directory path.");

				bucketName = path.Segments[0];
				return path.FullPath;
			}

			private string EnsurePatternPath(string path, string pattern, out string bucketName)
			{
				if(string.IsNullOrWhiteSpace(pattern))
					return this.EnsureDirectoryPath(path, out bucketName);

				return this.EnsureDirectoryPath(path, out bucketName) + pattern.Trim('*', ' ', '\t', '\r', '\n').TrimStart('/');
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
			#endregion

			#region 私有构造
			internal StorageFileProvider(StorageFileSystem fileSystem)
			{
				_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
			}
			#endregion

			#region 公共方法
			public bool Delete(string path)
			{
				return Utility.ExecuteTask(() => this.DeleteAsync(path));
			}

			public Task<bool> DeleteAsync(string path)
			{
				path = this.EnsureFilePath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);
				return client.DeleteAsync(path);
			}

			public bool Exists(string path)
			{
				return Utility.ExecuteTask(() => this.ExistsAsync(path));
			}

			public Task<bool> ExistsAsync(string path)
			{
				path = this.EnsureFilePath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);
				return client.ExistsAsync(path);
			}

			public void Copy(string source, string destination)
			{
				this.Copy(source, destination, false);
			}

			public void Copy(string source, string destination, bool overwrite)
			{
				this.CopyAsync(source, destination, overwrite).Wait();
			}

			public Task CopyAsync(string source, string destination)
			{
				return this.CopyAsync(source, destination, false);
			}

			public async Task CopyAsync(string source, string destination, bool overwrite)
			{
				source = this.EnsureFilePath(source, out var sourceBucket);
				destination = this.EnsureFilePath(destination, out var destinationBucket);

				if(string.Equals(sourceBucket, destinationBucket, StringComparison.OrdinalIgnoreCase))
				{
					var client = _fileSystem.GetClient(sourceBucket);

					if(!overwrite && await client.ExistsAsync(destination))
						return;

					await client.CopyAsync(source, destination);
				}
				else
				{
					var sourceClient = _fileSystem.GetClient(sourceBucket);
					var destinationClient = _fileSystem.GetClient(destinationBucket);

					if(!overwrite && await destinationClient.ExistsAsync(destination))
						return;

					using(var sourceStream = await sourceClient.DownloadAsync(source))
					{
						using(var uploader = destinationClient.GetUploader(destination))
						{
							var buffer = new byte[1024 * 64];
							var bytesRead = 0;

							while((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
							{
								uploader.Write(buffer, 0, bytesRead);
							}

							uploader.Flush();
						}
					}
				}
			}

			public void Move(string source, string destination)
			{
				this.MoveAsync(source, destination).Wait();
			}

			public async Task MoveAsync(string source, string destination)
			{
				source = this.EnsureFilePath(source, out var sourceBucket);
				destination = this.EnsureFilePath(destination, out var destinationBucket);

				if(string.Equals(sourceBucket, destinationBucket, StringComparison.OrdinalIgnoreCase))
				{
					var client = _fileSystem.GetClient(sourceBucket);

					if(await client.CopyAsync(source, destination))
						await client.DeleteAsync(source);
				}
				else
				{
					var sourceClient = _fileSystem.GetClient(sourceBucket);
					var destinationClient = _fileSystem.GetClient(destinationBucket);

					using(var sourceStream = await sourceClient.DownloadAsync(source))
					{
						using(var uploader = destinationClient.GetUploader(destination))
						{
							var buffer = new byte[1024 * 64];
							var bytesRead = 0;

							while((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
							{
								uploader.Write(buffer, 0, bytesRead);
							}

							uploader.Flush();
						}
					}

					await sourceClient.DeleteAsync(source);
				}
			}

			public Zongsoft.IO.FileInfo GetInfo(string path)
			{
				return Utility.ExecuteTask(() => this.GetInfoAsync(path));
			}

			public async Task<Zongsoft.IO.FileInfo> GetInfoAsync(string path)
			{
				path = this.EnsureFilePath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);

				var properties = await client.GetExtendedPropertiesAsync(path);

				return this.GenerateInfo(path, properties);
			}

			public bool SetInfo(string path, IDictionary<string, object> properties)
			{
				return Utility.ExecuteTask(() => this.SetInfoAsync(path, properties));
			}

			public async Task<bool> SetInfoAsync(string path, IDictionary<string, object> properties)
			{
				path = this.EnsureFilePath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);
				return await client.SetExtendedPropertiesAsync(path, properties);
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
				path = this.EnsureFilePath(path, out var bucketName);
				var client = _fileSystem.GetClient(bucketName);

				bool writable = (mode != FileMode.Open) || (access & FileAccess.Write) == FileAccess.Write;

				if(writable)
					return new StorageFileStream(client.GetUploader(path, properties));

				return Utility.ExecuteTask(() => client.DownloadAsync(path, properties));
			}
			#endregion

			#region 私有方法
			private string EnsureFilePath(string text, out string bucketName)
			{
				var path = Zongsoft.IO.Path.Parse(text);

				if(!path.IsFile)
					throw new PathException("Invalid file path.");

				bucketName = path.Segments[0];
				return path.FullPath;
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
