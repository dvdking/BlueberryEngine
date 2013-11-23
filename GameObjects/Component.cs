using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blueberry.GameObjects.Messages;
using System.ComponentModel;
using System.Reflection;
using System.Drawing;

namespace Blueberry.GameObjects
{
    public abstract class Component
    {
		public readonly string Name;

		public FieldInfo[] Fields { get; protected set; }

		public object this[string name]
		{
			get
			{
				return Fields.First(p => p.Name == name).GetValue(this);
			}
			set
			{
				Fields.First(p => p.Name == name).SetValue(this, value);
			}
		}
		public object this [FieldInfo field]
		{
			get
			{
				return this[field.Name];
			}
			set
			{
				this[field.Name] = value;
			}
		}
		
		public GameObject Owner { get; internal set; }

        public Component()
        {
			Fields = GetType().GetFields();
			Name = GetType().Name;
        }


		public virtual void ProccesMessage(IMessage message){
		}

        public virtual bool GetDependicies()
        {
            return true;
        }

		public bool SetFieldValue(string fieldName, string fieldValue)
		{
			FieldInfo info = Fields.First(p => p.Name == fieldName);
			if (info.FieldType == typeof(int))
			{
				int i = int.Parse(fieldValue);
				this[fieldName] = i;
				return true;
			}
			if (info.FieldType == typeof(float))
			{
				float f = float.Parse(fieldValue);
				this[fieldName] = f;
				return true;
			}
			if (info.FieldType == typeof(double))
			{
				float d = float.Parse(fieldValue);
				this[fieldName] = d;
				return true;
			}
			if (info.FieldType == typeof(string))
			{
				this[fieldName] = fieldValue;
				return true;
			}
			if (info.FieldType == typeof(GameObject))
			{
					this[fieldName] = Owner.GameObjectManager.GetByName(fieldValue);
					return true;
			}

			return false;
		}

		public Component Clone()
		{
			var type = GetType();
			var clone = (Component)type.GetConstructor(Type.EmptyTypes).Invoke(null); 
			var field = type.GetFields();
			foreach (var f in field)
			{
				f.SetValue(clone, f.GetValue(this));
			}
			return clone;
		}
    }
}
