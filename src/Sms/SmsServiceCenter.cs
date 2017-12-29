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

namespace Zongsoft.Externals.Aliyun.Sms
{
	public class SmsServiceCenter : ServiceCenterBase
	{
		#region 单例字段
		public static readonly SmsServiceCenter Beijing = new SmsServiceCenter(ServiceCenterName.Beijing);
		public static readonly SmsServiceCenter Qingdao = new SmsServiceCenter(ServiceCenterName.Qingdao);
		public static readonly SmsServiceCenter Hangzhou = new SmsServiceCenter(ServiceCenterName.Hangzhou);
		public static readonly SmsServiceCenter Shenzhen = new SmsServiceCenter(ServiceCenterName.Shenzhen);
		public static readonly SmsServiceCenter Hongkong = new SmsServiceCenter(ServiceCenterName.Hongkong);
		#endregion

		#region 构造函数
		private SmsServiceCenter(ServiceCenterName name) : base(name, false)
		{
			this.Path = "dysmsapi.aliyuncs.com";
		}
		#endregion

		#region 静态方法
		public static SmsServiceCenter GetInstance(ServiceCenterName name)
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
