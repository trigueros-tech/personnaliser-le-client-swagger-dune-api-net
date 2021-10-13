using System;

namespace Api.Infrastructure.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRightAttribute : Attribute
    {
        public string Right { get; }
        
        public RequireRightAttribute(string right)
        {
            Right = right;
        }
    }
}