﻿using System;
using System.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace BaggyBot
{
	class SqlConnector
	{
		private MySqlConnection connection;
		private string server;
		private string database;
		private string uid;
		private string password = (string) ConfigurationManager.AppSettings["sqlpass"];

		public SqlConnector()
		{
			Initialize();
		}

		private void Initialize()
		{
			server = "127.0.0.1";
			database = "stats_bot";
			uid = "statsbot";
			password = "HDD#94nl@ihm";
			string connectionString;
			connectionString = "SERVER=" + server + ";" + "DATABASE=" +
			database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

			connection = new MySqlConnection(connectionString);
		}

		public void InitializeDatabase()
		{
			Console.WriteLine("Attempting to initialize the database if this hasn't been done yet");

			List<string> createStmts = new List<string>();

			createStmts.Add( 
			@"CREATE  TABLE IF NOT EXISTS `usercreds` (
			  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT ,
			  `user_id` INT UNSIGNED NOT NULL ,
			  `nick` VARCHAR(45) NOT NULL ,
			  `ident` VARCHAR(16) NOT NULL ,
			  `hostmask` VARCHAR(128) NOT NULL ,
			  `ns_login` VARCHAR(45) NULL ,
			  PRIMARY KEY (`id`) ,
			  UNIQUE INDEX `id_UNIQUE` (`id` ASC) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE  TABLE IF NOT EXISTS `userstats` (
			  `user_id` INT UNSIGNED NOT NULL ,
			  `lines` INT UNSIGNED NOT NULL ,
			  `words` INT UNSIGNED NOT NULL ,
			  `actions` INT UNSIGNED NOT NULL ,
			  `profanities` INT UNSIGNED NOT NULL ,
			  PRIMARY KEY (`user_id`) ,
			  UNIQUE INDEX `user_id_UNIQUE` (`user_id` ASC) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE TABLE IF NOT EXISTS `quotes` (
			  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
			  `user_id` INT UNSIGNED NOT NULL ,
			  `quote` VARCHAR(500) NOT NULL ,
			  PRIMARY KEY (`id`) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE  TABLE IF NOT EXISTS `var` (
			  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT ,
			  `key` VARCHAR(45) NOT NULL ,
			  `value` INT UNSIGNED NOT NULL ,
			  PRIMARY KEY (`id`) ,
			  UNIQUE INDEX `id_UNIQUE` (`id` ASC) ,
			  UNIQUE INDEX `key_UNIQUE` (`key` ASC) );");

			foreach (string str in createStmts) {
				ExecuteStatement(str);
			}

			Logger.Log("Done.");
		}

		public bool OpenConnection()
		{
			try {
				connection.Open();
				return true;
			} catch (MySqlException ex) {
				Console.WriteLine("ERROR: " + ex.Message);
				return false;
			}
		}

		public bool CloseConnection()
		{
			try {
				connection.Close();
				return true;
			} catch (MySqlException ex) {
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Execute a statement without returning any data.
		/// </summary>
		/// <param name="statement">The statement to execute</param>
		/// <returns>Number of rows affected</returns>
		public int ExecuteStatement(string statement)
		{
			MySqlCommand cmd = new MySqlCommand(statement, connection);
			return cmd.ExecuteNonQuery();
		}

		/// <summary>
		/// Execute a query and tries to return the data as a DataView.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public DataView Select(string query)
		{
			MySqlCommand cmd = new MySqlCommand(query, connection);
			MySqlDataAdapter da = new MySqlDataAdapter(cmd);

			DataSet ds = new DataSet();
			da.Fill(ds);
			return ds.Tables[0].DefaultView;
		}

		public T[] SelectVector<T>(string query)
		{
			DataView dv = Select(query);
			if (dv.Table.Columns.Count > 1) {
				throw new InvalidOperationException("The passed query returned more than one column.");
			} else {
				T[] data = new T[dv.Count];
				for (int i = 0; i < data.Length; i++) {
					data[i] = (T) dv[0][i];
				}
				return data;
			}
		}

		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <returns></returns>
		public T SelectOne<T>(string query)
		{
			DataView dv = Select(query);
			if (dv.Count == 1) {
				Object data = dv[0][0];
				return (T)data;
			} else {
				throw new InvalidOperationException("The passed query returned more or less than one record.");
			}
		}
	}
}
