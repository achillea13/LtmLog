/*----------------------------------------------------------------------------------------------------
 * 
 *  	name 	:LtmLog
 *  	author 	:Liutangming
 *  	desc 	:Simple Log System
 * 
 * --------------------------------------------------------------------------------------------------*/

using UnityEngine;
using System.Collections.Generic;

namespace Ltm
{
	public enum LOG_TYPE
	{
		Unity = 1,
	}

	public abstract class ILog
	{

		protected bool _valid = true;
		protected string _id;
		protected LOG_TYPE _type;
		protected static Dictionary<LOG_TYPE, Dictionary<string,ILog>> s_dictLogs = new Dictionary<LOG_TYPE, Dictionary<string,ILog>>();

		public string id
		{ get{return _id;} }
		public LOG_TYPE type
		{get{return _type;}}

		public ILog(LOG_TYPE typ, string id)
		{
			_id = id;
		}

		public void SetValid(bool valid)
		{
			_valid = valid;
		}

		protected static void Add(ILog log)
		{
			if ( log == null )
				return;

			if ( s_dictLogs.ContainsKey(log.type) == false )
				s_dictLogs[log.type] = new Dictionary<string, ILog>();

			if ( s_dictLogs[log.type].ContainsKey(log.id) == false )
				s_dictLogs[log.type].Add( log.id, log);
		}

		public static ILog GetLog(LOG_TYPE typ, string id)
		{

			if ( s_dictLogs.ContainsKey(typ) && s_dictLogs[typ].ContainsKey(id) )
				return s_dictLogs[typ][id];

			ILog log = null;
			switch( typ )
			{
				case LOG_TYPE.Unity:
				{
				 	log = new UnityLog(id);
				 	Add(log);	
				}
				break;
			}

			return log;
		}



		
		public abstract void Log(params object[] objs);
		public abstract void LogWarning(params object[] objs);
		public abstract void LogError(params object[] objs);

	}

	public class UnityLog : ILog
	{

		public UnityLog(string id) : base(LOG_TYPE.Unity, id)
		{
		}

		public override void Log(params object[] objs)
		{
			if (_valid && objs.Length > 0 )
				Debug.Log(objs[0]);
		}
		public override void LogWarning( params object[] objs)
		{
			if (_valid && objs.Length >0)
				Debug.LogWarning(objs[0]);
		}
		public override void LogError(params object[] objs)
		{
			if (_valid && objs.Length >0)
				Debug.LogError(objs[0]);
		}

	}



	
}
