﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.HostedScript.Library;

namespace MyService.OneScriptBridge
{
    /// <summary>
    /// Абстракция для вывода куда-нибудь сообщений от Сообщить
    /// </summary>
    public interface IMessager
    {
        void PrintMessage(string message, MessageStatusEnum status);
    }
}