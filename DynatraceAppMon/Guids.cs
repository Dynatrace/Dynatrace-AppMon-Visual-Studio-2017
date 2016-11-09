using System;

namespace DynaTrace.CodeLink
{
    static class Guids
    {
        public const string DYNATRACE_VS2005_PACKAGE_STRING = "d828c52f-8f80-40e5-9905-4334b7879798";
        public const string DYNATRACE_VS2005_CMD_SET_STRING = "d8833e6f-197c-4417-ae3b-31478dbf6ce1";

        public static readonly Guid DYNATRACE_VS2005_PACKAGE = new Guid(DYNATRACE_VS2005_PACKAGE_STRING);
        public static readonly Guid DYNATRACE_VS2005_CMD_SET = new Guid(DYNATRACE_VS2005_CMD_SET_STRING);
    };
}