﻿using Flurl.Http;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Utilities
{
    /// <summary>
    /// This class manages cookie sessions per site on a global level.
    /// </summary>
    public class GlobalCookieJar : IDisposable
    {
        public Dictionary<string, CookieSession> sessions;
        public IDisposable dictionaryDisposer;

        public GlobalCookieJar()
        {
            sessions = new Dictionary<string, CookieSession>();
            dictionaryDisposer = sessions.GetDisposer();
        }

        public CookieSession this[string url] => (sessions.TryGetValue(url, out var res)) ?  res : sessions.Add<string,CookieSession>(url, new CookieSession(url));

        public void Dispose()
        {
            dictionaryDisposer?.Dispose();
        }
    }
}
