using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


namespace LSystem
{
	public static class Serializer
	{
		public static string Extension = ".ls";


		/// <summary>
		/// Serializes the given state to the given file.
		/// Returns an error message, or an empty string if everything went OK.
		/// </summary>
		public static string ToFile(LState state, string path)
		{
			Stream fileS = null;
			try
			{
				fileS = File.Open(path, FileMode.Create);

				BinaryFormatter bFormatter = new BinaryFormatter();
				bFormatter.Serialize(fileS, state);

				fileS.Close();
			}
			catch (Exception e)
			{
				return e.Message;
			}
			finally
			{
				if (fileS != null)
					fileS.Close();
			}

			return "";
		}
		/// <summary>
		/// Reads an LState from the given file into the given variable.
		/// Returns an error message, or an empty string if everything went OK.
		/// </summary>
		public static string FromFile(string path, out LState outState)
		{
			outState = new LState();

			Stream fileS = null;
			try
			{
				fileS = File.Open(path, FileMode.Open);
				BinaryFormatter bForm = new BinaryFormatter();
				outState = (LState)bForm.Deserialize(fileS);
				fileS.Close();
			}
			catch (Exception e)
			{
				return e.Message;
			}
			finally
			{
				if (fileS != null)
					fileS.Close();
			}

			return "";
		}
	}
}