﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Utilities
{
    public static class TaskExtensions
    {
        public static async Task<T> WithRetries<T>(this Task<T> task, int retries)
        {
            Exception temp = new Exception();
            for (int i = 0; i <= retries; i++)
                try
                {
                    return await task;
                } catch (Exception e)
                {
                    if (i == retries) temp = e;
                }
            throw temp;
        }
    }
}
