using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.PostEffects
{
    public static class PostEffectMgr
    {
        static List<PostEffect> _currentPostEffects = new List<PostEffect>();
        static List<PostEffect> _loadedPostEffects = new List<PostEffect>();

        public static void AddEffect(string name)
        {
            PostEffect p = _loadedPostEffects.Find(t => t.Name == name);
            _currentPostEffects.Add(p);
        }

        public static void ResetActiveEffects()
        {
            _currentPostEffects.Clear();
        }

        static internal void PostEffectBegin()
        {
 
        }

        static internal void PostEffectEnd()
        {
 
        }
    }
}
