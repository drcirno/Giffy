using System;
using System.Collections.Generic;
using System.Windows;

namespace Giffy
{
	public class CelerityIO
	{
		private List<CeleritySpell> cspell = new List<CeleritySpell>();

		public CelerityIO()
		{
			// Empty constructor
		}

		public List<CeleritySpell> readSpellList()
		{
			string line = "";
			int counter = 0;
			CeleritySpell previous = null, current = null;
			string filename = "database\\celerity-spells.txt";

			System.IO.StreamReader file = new System.IO.StreamReader(filename);
			while ((line = file.ReadLine()) != null)
			{
				string[] stringList = line.Split('$');
				current = new CeleritySpell(stringList[0], stringList[1], stringList[2], stringList[3], stringList[4], stringList[5],
								   stringList[6], stringList[7], stringList[8], stringList[9], stringList[10],
								   stringList[11]);
				current.body.Replace("&", "\n\n");

				if (previous == null) {
					cspell.Add(current);
					previous = current;
				}
				else {
					if (previous.Equals(current))
					{
						cspell[counter].addSpellLevel(current.getSpellLevel()[0]);
					}
					else 
					{
						cspell.Add(current);
						previous = current;
						counter++;
					}
				}

			}

			file.Close();
			return this.cspell;
		}

	}
}