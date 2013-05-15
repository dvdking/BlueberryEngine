using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Blueberry.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace Blueberry.SceneManagement
{
    public class Scene
    {
        public static bool designer_mode = false;
        private bool need_sort;

        List<Actor> actors;
        Camera camera;

        public Camera CurrentCamera { get { return camera; } }

        public List<Actor> Actors { get { need_sort = true; return actors; } }

        public Scene()
        {
            actors = new List<Actor>();
            need_sort = false;
        }

        public void AttachCamera(Camera camera)
        {
            this.camera = camera;
        }

        public void InsertActor(Actor actor)
        {
            if (actor.Scene != null)
                actor.Scene.RemoveActor(actor);
            if (!actors.Contains(actor))
            {
                actors.Add(actor);
                actor.Scene = this;
            }
            need_sort = true;
        }

        public void RemoveActor(Actor actor)
        {
            if (actors.Contains(actor))
            {
                actors.Remove(actor);
                actor.Scene = null;
            }
            need_sort = true;
        }

        public List<IBoundedActor> Get(Vector2 point)
        {
            List<IBoundedActor> r = new List<IBoundedActor>();
            foreach (var item in actors)
            {
                if (item is IBoundedActor)
                {
                    Vector2 p = camera.ToWorld(point, item.ParallaxLayer);
                    if (((IBoundedActor)item).Bounds.Contains(p.X, p.Y))
                        r.Add((IBoundedActor)item);
                }
            }
            need_sort = true;
            return r;
        }

        public List<IBoundedActor> Get(RectangleF area, bool onlyContained)
        {
            List<IBoundedActor> r = new List<IBoundedActor>();
            foreach (var item in actors)
            {
                if (item is IBoundedActor)
                {
                    PointF p1 = camera.ToWorld(area.Location, item.ParallaxLayer);
                    PointF p2 = camera.ToWorld(new PointF(area.Right, area.Bottom), item.ParallaxLayer);
                    RectangleF rect = new RectangleF(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
                    if (onlyContained)
                    {
                        if (rect.Contains(((IBoundedActor)item).Bounds))
                            r.Add((IBoundedActor)item);
                    }
                    else
                    {
                        if (rect.IntersectsWith(((IBoundedActor)item).Bounds))
                            r.Add((IBoundedActor)item);
                    }
                }
            }
            need_sort = true;
            return r;
        }

        public virtual void Update(float dt)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].Update(dt);
            }
            camera.Update(dt);
        }

        public virtual void Load()
        {
            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].Load();
            }
            need_sort = true;
        }

        public virtual void Draw(float dt)
        {
            if (need_sort)
            {
                actors.TimSort((a, b) => a.ParallaxLayer < b.ParallaxLayer ? -1 : a.ParallaxLayer > b.ParallaxLayer ? 1 : 0);
                need_sort = false;
            }
            if (camera != null)
            {
                float parallax = actors[0].ParallaxLayer;
                SpriteBatch.Please.Begin(camera.GetViewMatrix(parallax));

                for (int i = 0; i < actors.Count; i++)
                {
                    if (parallax != actors[i].ParallaxLayer)
                    {
                        SpriteBatch.Please.End();
                        parallax = actors[i].ParallaxLayer;
                        SpriteBatch.Please.Begin(camera.GetViewMatrix(parallax));
                    }
                    actors[i].Draw(dt);
                }
                SpriteBatch.Please.End();
            }
        }
    }
}