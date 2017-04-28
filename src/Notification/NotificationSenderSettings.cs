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
	public class NotificationSenderSettings
	{
		#region 成员字段
		private NotificationType _type;
		private NotificationDeviceType _deviceType;
		private NotificationTargetType _targetType;
		#endregion

		#region 构造函数
		public NotificationSenderSettings()
		{
			_type = NotificationType.Message;
			_deviceType = NotificationDeviceType.All;
			_targetType = NotificationTargetType.Alias;
		}

		public NotificationSenderSettings(NotificationType type, NotificationDeviceType deviceType, NotificationTargetType targetType)
		{
			_type = type;
			_deviceType = deviceType;
			_targetType = targetType;
		}
		#endregion

		#region 公共属性
		public NotificationType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public NotificationDeviceType DeviceType
		{
			get
			{
				return _deviceType;
			}
			set
			{
				_deviceType = value;
			}
		}

		public NotificationTargetType TargetType
		{
			get
			{
				return _targetType;
			}
			set
			{
				_targetType = value;
			}
		}
		#endregion
	}
}
