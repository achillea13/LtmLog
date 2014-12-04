/*----------------------------------------------------------------------------------------------------
 * 
 *  	name 	:LtmLog
 *  	author 	:Liutangming
 *  	desc 	:Simple Log System
 * 
 * --------------------------------------------------------------------------------------------------*/


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
			_type = typ;
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
#if UNITY_EDITOR
				 	log = new UnityLog(id);
				 	Add(log);	
#endif
				}
				break;
			}

			return log;
		}






		// 寄生类需要实现的接口
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
#if UNITY_EDITOR
			if (_valid && objs.Length > 0 )
				UnityEngine.Debug.Log(objs[0]);
#endif
		}
		public override void LogWarning( params object[] objs)
		{
#if UNITY_EDITOR
			if (_valid && objs.Length >0)
				UnityEngine.Debug.LogWarning(objs[0]);
#endif
		}
		public override void LogError(params object[] objs)
		{
#if UNITY_EDITOR
			if (_valid && objs.Length >0)
				UnityEngine.Debug.LogError(objs[0]);
#endif
		}

	}



	
}
