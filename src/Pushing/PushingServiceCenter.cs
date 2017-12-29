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

namespace Zongsoft.Externals.Aliyun.Pushing
{
	public class PushingServiceCenter : ServiceCenterBase
	{
		#region 单例字段
		public static readonly PushingServiceCenter Beijing = new PushingServiceCenter(ServiceCenterName.Beijing);
		public static readonly PushingServiceCenter Qingdao = new PushingServiceCenter(ServiceCenterName.Qingdao);
		public static readonly PushingServiceCenter Hangzhou = new PushingServiceCenter(ServiceCenterName.Hangzhou);
		public static readonly PushingServiceCenter Shenzhen = new PushingServiceCenter(ServiceCenterName.Shenzhen);
		public static readonly PushingServiceCenter Hongkong = new PushingServiceCenter(ServiceCenterName.Hongkong);
		#endregion

		#region 构造函数
		private PushingServiceCenter(ServiceCenterName name) : base(name, false)
		{
			this.Path = "cloudpush.aliyuncs.com";
		}
		#endregion

		#region 静态方法
		public static PushingServiceCenter GetInstance(ServiceCenterName name)
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
