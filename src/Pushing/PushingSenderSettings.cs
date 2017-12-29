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

namespace Zongsoft.Externals.Aliyun.Pushing
{
	/// <summary>
	/// 表示移动推送的设置选项类。
	/// </summary>
	public class PushingSenderSettings
	{
		#region 成员字段
		private int _expiry;
		private PushingType _type;
		private PushingDeviceType _deviceType;
		private PushingTargetType _targetType;
		#endregion

		#region 构造函数
		public PushingSenderSettings()
		{
			_expiry = 60 * 72;
			_type = PushingType.Message;
			_deviceType = PushingDeviceType.All;
			_targetType = PushingTargetType.Alias;
		}

		public PushingSenderSettings(PushingType type, PushingDeviceType deviceType, PushingTargetType targetType, int expiry = 0)
		{
			_expiry = expiry < 0 ? 60 * 72 : expiry;
			_type = type;
			_deviceType = deviceType;
			_targetType = targetType;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置移动推送消息或通知的过期时间，即当指定的目标不在线的情况下保存的有效期（单位：分钟）。
		/// </summary>
		public int Expiry
		{
			get
			{
				return _expiry;
			}
			set
			{
				_expiry = value;
			}
		}

		/// <summary>
		/// 获取或设置移动推送的类型（消息或通知），默认值为消息(Message)。
		/// </summary>
		public PushingType Type
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

		/// <summary>
		/// 获取或设置移动推送的设备类型，默认值为所有(All)。
		/// </summary>
		public PushingDeviceType DeviceType
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

		/// <summary>
		/// 获取或设置移动推送的目标（即推送方式）。
		/// </summary>
		public PushingTargetType TargetType
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
