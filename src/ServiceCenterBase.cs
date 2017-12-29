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

namespace Zongsoft.Externals.Aliyun
{
	/// <summary>
	/// 表示服务中心的基类。
	/// </summary>
	public class ServiceCenterBase
	{
		#region 成员字段
		private ServiceCenterName _name;
		private string _alias;
		private string _path;
		#endregion

		#region 构造函数
		protected ServiceCenterBase(ServiceCenterName name, bool isInternal)
		{
			_name = name;

			switch(name)
			{
				case ServiceCenterName.Beijing: //北京服务中心
					_alias = "cn-beijing";
					_path = isInternal ? "beijing-internal.aliyuncs.com" : "beijing.aliyuncs.com";
					break;
				case ServiceCenterName.Qingdao: //青岛服务中心
					_alias = "cn-qingdao";
					_path = isInternal ? "qingdao-internal.aliyuncs.com" : "qingdao.aliyuncs.com";
					break;
				case ServiceCenterName.Hangzhou: //杭州服务中心
					_alias = "cn-hangzhou";
					_path = isInternal ? "hangzhou-internal.aliyuncs.com" : "hangzhou.aliyuncs.com";
					break;
				case ServiceCenterName.Shenzhen: //深圳服务中心
					_alias = "cn-shenzhen";
					_path = isInternal ? "shenzhen-internal.aliyuncs.com" : "shenzhen.aliyuncs.com";
					break;
				case ServiceCenterName.Hongkong: //香港服务中心
					_alias = "cn-hongkong";
					_path = isInternal ? "hongkong-internal.aliyuncs.com" : "hongkong.aliyuncs.com";
					break;
			}
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取服务中心的名称。
		/// </summary>
		public ServiceCenterName Name
		{
			get
			{
				return _name;
			}
		}

		/// <summary>
		/// 获取或设置服务中心的别名。
		/// </summary>
		public string Alias
		{
			get
			{
				return _alias;
			}
			protected set
			{
				_alias = value;
			}
		}

		/// <summary>
		/// 获取或设置服务中心的访问路径。
		/// </summary>
		public virtual string Path
		{
			get
			{
				return _path;
			}
			protected set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_path = value.Trim();
			}
		}
		#endregion
	}
}
