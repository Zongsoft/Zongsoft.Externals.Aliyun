/*
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

namespace Zongsoft.Externals.Aliyun.Notification
{
	public class NotificationServiceCenter : ServiceCenter
	{
		#region 单例字段
		public static readonly NotificationServiceCenter Beijing = new NotificationServiceCenter(ServiceCenterName.Beijing);
		public static readonly NotificationServiceCenter Qingdao = new NotificationServiceCenter(ServiceCenterName.Qingdao);
		public static readonly NotificationServiceCenter Hangzhou = new NotificationServiceCenter(ServiceCenterName.Hangzhou);
		public static readonly NotificationServiceCenter Shenzhen = new NotificationServiceCenter(ServiceCenterName.Shenzhen);
		public static readonly NotificationServiceCenter Hongkong = new NotificationServiceCenter(ServiceCenterName.Hongkong);
		#endregion

		#region 成员字段
		private string _region;
		#endregion

		#region 构造函数
		private NotificationServiceCenter(ServiceCenterName name) : base(name, false)
		{
			this.Path = "cloudpush.aliyuncs.com";

			switch(name)
			{
				case ServiceCenterName.Beijing:
					_region = "cn-beijing";
					break;
				case ServiceCenterName.Qingdao:
					_region = "cn-qingdao";
					break;
				case ServiceCenterName.Hangzhou:
					_region = "cn-hangzhou";
					break;
				case ServiceCenterName.Shenzhen:
					_region = "cn-shenzhen";
					break;
				case ServiceCenterName.Hongkong:
					_region = "cn-hongkong";
					break;
			}
		}
		#endregion

		#region 公共属性
		public string Region
		{
			get
			{
				return _region;
			}
		}
		#endregion

		#region 静态方法
		public static NotificationServiceCenter GetInstance(ServiceCenterName name)
		{
			switch(name)
			{
				case ServiceCenterName.Beijing:
					return Beijing;
				case ServiceCenterName.Qingdao:
					return Qingdao;
				case ServiceCenterName.Hangzhou:
					return Hangzhou;
				case ServiceCenterName.Shenzhen:
					return Shenzhen;
				case ServiceCenterName.Hongkong:
					return Hongkong;
			}

			return null;
		}
		#endregion
	}
}
