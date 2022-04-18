using System;
using UnityEngine;
using System.Collections.Generic;

namespace Pacmetricas_G01 
{


    [System.Flags]
    public enum EventTypes {
		NONE = 0, ALL_EVENTS = ~0,
        INIT_GAME = 1<<0, END_GAME = 1<<1, MENU_PASSED = 1<<2, FIRST_PHRASE = 1<<3, CORRECT_DIR = 1<<4, INIT_RUN = 1<<5, PLAYER_DEAD = 1<<6,
        TRY_PHRASE_MENU = 1<<7, TRY_PHRASE_TAXI = 1<<8, DIRECTION_TAXI = 1<<9, VOLUME_MIC = 1<<10, BLACKOUT_INTENSITY = 1<<11, PLAYER_WON = 1<<12
    }

	public abstract class Event {
		public long gameSession;
		public long timeStamp;
		public string type;

		public Event(){
			gameSession = UnityEngine.Analytics.AnalyticsSessionInfo.sessionId;
			timeStamp =  ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
		}

		public abstract string ToJSON();
		public abstract string ToCSV();
			
		public static Dictionary <string, EventTypes> EventIDs = new Dictionary<string, EventTypes>()
		{
			{ "INIT_GAME", EventTypes.INIT_GAME },
			{ "END_GAME", EventTypes.END_GAME },
			{ "MENU_PASSED", EventTypes.MENU_PASSED },
			{ "FIRST_PHRASE", EventTypes.FIRST_PHRASE },
			{ "CORRECT_DIR", EventTypes.CORRECT_DIR },
			{ "INIT_RUN", EventTypes.INIT_RUN },
			{ "PLAYER_DEAD", EventTypes.PLAYER_DEAD },
			{ "PLAYER_WON", EventTypes.PLAYER_WON },
			{ "TRY_PHRASE_MENU", EventTypes.TRY_PHRASE_MENU },
			{ "TRY_PHRASE_TAXI", EventTypes.TRY_PHRASE_TAXI },
			{ "DIRECTION_TAXI", EventTypes.DIRECTION_TAXI },
			{ "VOLUME_MIC", EventTypes.VOLUME_MIC },
			{ "BLACKOUT_INTENSITY", EventTypes.BLACKOUT_INTENSITY },
			{ "ALL_EVENTS", EventTypes.ALL_EVENTS }
		};
	}

	///////////////////////////////////////////////////////////////////////////////////////////
	//Eventos simples que solo cuentan con el tipo de evento, el timeStamp y el id de la sesion
	public class TimeStampEvent: Event {
		
		public override string ToJSON(){
			return JsonUtility.ToJson(this, true);
		}

		public override string ToCSV(){
			return type + "," + timeStamp + "," + gameSession + ",,\n";
		}
	}

	public class InitGameEvent: TimeStampEvent {
		
		public InitGameEvent(){
			type = "INIT_GAME";
		}
	}

	public class EndGameEvent : TimeStampEvent
	{

		public EndGameEvent()
		{
			type = "END_GAME";
		}
	}

	public class MenuPassedEvent: TimeStampEvent {
		
		public MenuPassedEvent(){
			type = "MENU_PASSED";
		}

	}

	public class FirstPhraseEvent: TimeStampEvent {
		
		public FirstPhraseEvent(){
			type = "FIRST_PHRASE";
		}
	}

	public class CorrectDirectionEvent: TimeStampEvent {
		
		public CorrectDirectionEvent(){
			type = "CORRECT_DIR";
		}
	}

	public class InitRunEvent: TimeStampEvent {
		
		public InitRunEvent(){
			type = "INIT_RUN";
		}
	}

	public class PlayerDeadEvent: TimeStampEvent {
		
		public PlayerDeadEvent(){
			type = "PLAYER_DEAD";
		}
	}

	public class PlayerWonEvent : TimeStampEvent
	{

		public PlayerWonEvent()
		{
			type = "PLAYER_WON";
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////
	//Eventos que ademas incluyen frases que dice el jugador para intentar hacer alguna accion

	public class PhraseEvent: Event {
		public string phrase;
		
		public override string ToJSON(){
			return JsonUtility.ToJson(this, true);
		}

		public override string ToCSV(){
			return type + "," + timeStamp + "," + gameSession + "," + phrase + ",\n";
		}
	}

	public class PhraseMenuEvent: PhraseEvent {
		
		public PhraseMenuEvent(){
			type = "TRY_PHRASE_MENU";
			phrase = "";
		}

		public PhraseMenuEvent(string tryphrase) {
			type = "TRY_PHRASE_MENU";
			phrase = tryphrase;
		}
	}


	public class PhraseTaxiEvent: PhraseEvent {
		
		public PhraseTaxiEvent(){
			type = "TRY_PHRASE_TAXI";
			phrase = "";
		}

		public PhraseTaxiEvent(string tryphrase) {
			type = "TRY_PHRASE_TAXI";
			phrase = tryphrase;
		}
	}

	public class DirectionTaxiEvent: PhraseEvent {
		public DirectionTaxiEvent(){
			type = "DIRECTION_TAXI";
			phrase = "";
		}

		public DirectionTaxiEvent(string tryphrase) {
			type = "DIRECTION_TAXI";
			phrase = tryphrase;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////
	//Estos eventos mandan ademas un valor de 0.0 a 1.0 que indica cierto parametro del juego

	public class ValueEvent: Event {
		public float value;
		
		public override string ToJSON(){
			return JsonUtility.ToJson(this, true);
		}

		public override string ToCSV(){
			return type + "," + timeStamp + "," + gameSession + ",," + value  + "\n";
		}
	}

	public class MicrophoneVolume: ValueEvent {
		
		public MicrophoneVolume(){
			type = "VOLUME_MIC";
			value = -1.0f; //Usamos -1 como valor por defecto para saber si es erroneo
		}

		public MicrophoneVolume(float micValue) {
			type = "VOLUME_MIC";
			value = micValue;
		}
	}

	public class BlackoutIntensityVolume: ValueEvent {
		
		public BlackoutIntensityVolume(){
			type = "BLACKOUT_INTENSITY";
			value = -1.0f; //Usamos -1 como valor por defecto para saber si es erroneo
		}

		public BlackoutIntensityVolume(float blackoutIntensity) {
			type = "BLACKOUT_INTENSITY";
			value = blackoutIntensity;
		}
	}
}