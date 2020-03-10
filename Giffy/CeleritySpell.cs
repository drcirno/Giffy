using System;
using System.Collections.Generic;

namespace Giffy 
{
	public class CeleritySpell
	{
		public string link { get; set; }
		public string title { get; set; }
		public string category { get; set; }
		public List<SpellLevel> spellLevel = new List<SpellLevel>();
		public string components { get; set; }
		public string castingTime { get; set; }
		public string range { get; set; }
		public string target { get; set; }
		public string duration { get; set; }
		public string effect { get; set; }
		public string savingThrow { get; set; }
		public string body { get; set; }
		/**
		 * Constructor
		 */
		public CeleritySpell(String link, string title, string category, string spellLevel, string components, string castingTime, string range, string target, string duration, string effect, string savingThrow, string body)
		{
			try
			{
				if (link[0].Equals('"'))
					link = link.Substring(1, link.Length - 2);
				this.link = link;
				if (title[0].Equals('"'))
					title = title.Substring(1, title.Length - 2);
				this.title = title;
				if (category[0].Equals('"'))
					category = category.Substring(1, category.Length - 2);
				this.category = category;
				SpellLevel initial = new SpellLevel(spellLevel.Substring(0, spellLevel.Length - 2), Int32.Parse(spellLevel.Substring(spellLevel.Length - 1, 1)));
				this.spellLevel.Add(initial);
				if (components[0].Equals('"'))
					components = components.Substring(1, components.Length - 2);
				this.components = components;
				if (castingTime[0].Equals('"'))
					castingTime = castingTime.Substring(1, castingTime.Length - 2);
				this.castingTime = castingTime;
				if (range[0].Equals('"'))
					range = range.Substring(1, range.Length - 2);
				this.range = range;
				if (target[0].Equals('"'))
					target = target.Substring(1, target.Length - 2);
				this.target = target;
				if (duration[0].Equals('"'))
					duration = duration.Substring(1, duration.Length - 2);
				this.duration = duration;
				if (effect[0].Equals('"'))
					effect = effect.Substring(1, effect.Length - 2);
				this.effect = effect;
				if (savingThrow[0].Equals('"'))
					savingThrow = savingThrow.Substring(1, savingThrow.Length - 2);
				this.savingThrow = savingThrow;
				if (body[0].Equals('"'))
					body = body.Substring(1, body.Length - 2);
				this.body = body;
				if (this.body.Length > 997) this.body = this.body.Substring(0, 997) + "...";
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

		}

		/**
		 * I only need to compare the title to see if the spell is equal.
		 */
		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			CeleritySpell comparingObj = obj as CeleritySpell;
			if (comparingObj == null) return false;
			return this.title.Equals(comparingObj.title);
		}

		public void addSpellLevel(SpellLevel sl)
		{
			this.spellLevel.Add(sl);
		}

		public List<SpellLevel> getSpellLevel() 
		{
			return this.spellLevel;
		}

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public string spellLevelsToString() 
		{
			List<string> sb = new List<string>();
			foreach (SpellLevel sl in this.spellLevel) {
				sb.Add(sl.ToString());
			}
			return String.Join(", ", sb);
		}

		public override string ToString()
		{
			string ret = "";
			ret = ret + "Link: " + this.link + "\n";
			ret = ret + "Title: " + this.title + "\n";
			ret = ret + "Category: " + this.category + "\n";
			ret = ret + "Spell Level: " + this.spellLevelsToString() + "\n";
			ret = ret + "Components: " + this.components + "\n";
			ret = ret + "Casting Time: " + this.castingTime + "\n";
			ret = ret + "Range: " + this.range + "\n";
			ret = ret + "Target: " + this.target + "\n";
			ret = ret + "Duration: " + this.duration + "\n";
			ret = ret + "Effect: " + this.effect + "\n";
			ret = ret + "Saving Throw: " + this.savingThrow + "\n";
			ret = ret + "Description: " + "Char Count: " + this.body.Length.ToString() + "\n";

			return ret;
		}



		/**
		 * Private Inner Class of Spell Level
		 */
		public class SpellLevel
		{
			public string caster { get; set; }
			public int level { get; set; }

			public SpellLevel(string caster, int level)
			{
				this.caster = caster;
				this.level = level;
			}

			public override string ToString()
			{
				return this.caster + " " + level.ToString();
			}
		}
	}
}

