/*----------------------------------------------------------------------------------------------------
 * 
 *  	name 	:LtmLog
 *  	author 	:Liutangming
 *  	desc 	:Simple Log System
 * 
 * --------------------------------------------------------------------------------------------------*/

using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace Ltm
{

	// 日志类型
	public enum LOG_TYPE
	{
		Null = 0, 							// null
		Unity = 1, 							// unity环境下日志类型
		Txt, 								// 录入文件日志类型
	}

	// 日志等级
	public enum LOG_LV
	{
		Usual = 1, 							// 普通
		Warning, 							// 警告
		Error, 								// 错误
	}

	public static class LtmLog
	{
	
		// 管理所有Log对象的集合
		private static Hashtable s_tabLogs = new Hashtable();

		// 管理所有默认的LOg对象
		private static Hashtable s_tabLogsDefault = new Hashtable();
	
		// 当前指定的LogType
		private static LOG_TYPE _type = LOG_TYPE.Txt;

		private static string GenKey( LOG_TYPE typ, string id)
		{
			return typ + "__" + id;
		}

		// 获取一个类型的日志对象，如果不存在，则创建
		private static ILog GenLog(LOG_TYPE typ, string id)
		{

			ILog log = null;
			switch( typ )
			{
				case LOG_TYPE.Unity:
				{
#if UNITY_EDITOR
				 	log = new UnityLog(id);
#endif
				}
				break;

				case LOG_TYPE.Txt:
				{
					log = new FileLog(id);
				}
				break;
			}

			return log;
		}

		// 指定一些参数
		public static void Init( LOG_TYPE typ )
		{
			_type = typ;
			string key = GenKey(typ,"default");
			if ( s_tabLogsDefault.ContainsKey(key) == false )
			 	s_tabLogsDefault[key] = GenLog(_type,"default");

		}


		// 静态的日志写入接口
		// fileIn 	: 0:重设为no     1:重设为yes    其他值：不变
		public static void WriteLog(object msg, string id = null, LOG_LV lv = LOG_LV.Usual, int fileIn = 2 )
		{
			ILog log = null;
			if ( id == null ) 
			{
				string key = GenKey( _type, "default");
				log = (ILog)s_tabLogsDefault[key];
			}
			else
			{
				string key = GenKey( _type, id);
				if ( s_tabLogs.ContainsKey(key) )
					log = (ILog)s_tabLogs[key];
				else
				{
					log = GenLog(_type, id);
					s_tabLogs[key] = log;
				}
			}

			switch( fileIn )
			{
				case 0:
				{
					log.SetFileIn(false);
				}
				break;
				case 1:
				{
					log.SetFileIn(true);
				}
				break;
	
			}

			log.WriteLogDo(msg, lv);
			return;
		}

		private static void Turn( string id, bool bTurn )
		{
			string key = GenKey(_type, id);
			if ( s_tabLogs.ContainsKey(key) && s_tabLogs[key] != null )
				((ILog)s_tabLogs[key]).SetValid(bTurn);
		}

		private static void TurnAll(bool bTurn)
		{
			foreach( DictionaryEntry item in s_tabLogs )
			{
				if ( item.Value != null && (ILog)(item.Value) != null )
					((ILog)(item.Value)).SetValid(bTurn);
			}
		}

		// disable log by id
		// if param id is not sig , the last log object will be del
		public static void Disable( string id)
		{
			Turn(id,false);
		}

		public static void DisableAll()
		{
			TurnAll(false);
		}

		public static void Enable( string id )
		{
			Turn(id,true);


		}

		public static void EnableAll()
		{
			TurnAll(true);
		}








/*---------------------------------------------------------------------------------------------------
 * -------------------------------  Base log class 		-------------------------------------------*/

		private abstract class ILog
		{
			// 当前LOG对象id
			public string _id;
			
			// 当前LOG对象Type
			public LOG_TYPE _type;

			// 是否同时开启文件写入，默认关闭
			public bool _isFileIn = false;
			protected FileLog _fileLog = null;

			protected bool _valid = true;


			// 是否激活
			public virtual void SetValid(bool valid)
			{
				_valid = valid;
			}


			public ILog(LOG_TYPE typ, string id)
			{
				_type = typ;
				_id = id;
			}

			// 设置是否同时开启文件写入
			public void SetFileIn( bool bIn = true )
			{
				// 若本身就是文件日志类型，则直接返回
				if ( _type == LOG_TYPE.Txt )
				{
					_isFileIn = false;
					return;
				}
				
				_isFileIn = bIn;
				
				if ( bIn && _fileLog == null )
				{
					_fileLog = new FileLog(_id);
				}
			}

			// 留给manager直接调用的日志录入接口
			public void WriteLogDo(object msg, LOG_LV lv = LOG_LV.Usual)
			{
				if (!_valid)
					return;

				string msgLog = (string)msg;
				
				switch( lv )
				{
				case LOG_LV.Usual:
				{
					Log(msgLog);
				}
					break;
					
				case LOG_LV.Warning:
				{
					LogWarning(msgLog);
				}
					break;
					
				case LOG_LV.Error:
				{
					LogError(msgLog);
				}
					break;
				}
				
				
				// 如果同时开启了文件录入，则同时写入文件
				if ( _isFileIn && _fileLog != null )
				{
					_fileLog.WriteLogDo(msgLog);
				}
			}

			// 寄生类需要内部实现的录入接口
			protected abstract void Log(object obj);
			protected abstract void LogWarning(object obj);
			protected abstract void LogError(object obj);
		}


/* -------------------------------  Base log class end	---------------------------------------------
/*-------------------------------------------------------------------------------------------------*/



/*--------------------------------------------------------------------------------------------------
 * ------------------------------- 	Unity3D type log 	--------------------------------------------*/
		// 用于Unity编辑器下的Log类型
		private class UnityLog : ILog
		{
			
			public UnityLog(string id) : base(LOG_TYPE.Unity, id)
			{
			}
			
			protected override void Log(object obj)
			{
				#if UNITY_EDITOR
				UnityEngine.Debug.Log(obj);
				#endif
			}
			protected override void LogWarning(object obj)
			{
				#if UNITY_EDITOR
				UnityEngine.Debug.LogWarning(obj);
				#endif
			}
			protected override void LogError(object obj)
			{
				#if UNITY_EDITOR
				UnityEngine.Debug.LogError(obj);
				#endif
			}
		}
/*-------------------------------	Unity3D type log end --------------------------------------------
 * -------------------------------------------------------------------------------------------------*/



/*--------------------------------------------------------------------------------------------------
 * ------------------------------- 	File type log 		--------------------------------------------*/
		// 写入文件LOG
		private class FileLog : ILog
		{
			
			protected StreamWriter _streamWriter = null;
			protected string _file = null;
			
			public FileLog(string id) : base( LOG_TYPE.Txt, id)
			{
				_file = GenFileName(id);
			}
			
			~FileLog()
			{
				Close();
			}
			
			// 生成文件名，同时如果不存在路径，则生成
			protected string GenFileName( string name, string ext = "txt")
			{
				string path = System.Environment.CurrentDirectory;
				int idx = path.LastIndexOf('\\');
				string newFile;
				string newPath;
				if ( idx > 0 )
				{
					newPath = path + "\\Log\\";
					newFile = newPath + name + "." + ext;
				}
				else
				{
					newPath = path + "Log\\" ;
					newFile = newPath + name + "." + ext;
				}
				
				if (Directory.Exists(newPath)== false)
				{
					Directory.CreateDirectory(newPath);
				}
				
				return newFile;
			}
			
			// 打开对应文件写入流
			protected bool Open()
			{
				if ( _streamWriter != null )
					return true;
				
				if ( !File.Exists(_file) )
				{
					_streamWriter = new StreamWriter(_file,true);
				}
				else
				{
					_streamWriter = new StreamWriter(_file, true);
				}
				
				return true;
			}
			
			protected bool Close()
			{
				if( _streamWriter != null )
				{
					_streamWriter.Close();
				}
				
				_streamWriter = null;
				
				return true;
			}
			
			
			protected override void Log(object obj)
			{
				
				Open();
				_streamWriter.WriteLine(obj);
				Close();
			}
			
			protected override void LogWarning(object obj)
			{
				Open();
				_streamWriter.WriteLine(obj);
				Close();
			}
			
			protected override void LogError(object obj)
			{
				Open();
				_streamWriter.WriteLine(obj);
				Close();
			}
			
			
		}
/*-------------------------------	File type log end 	--------------------------------------------
 * -------------------------------------------------------------------------------------------------*/


	}

}
