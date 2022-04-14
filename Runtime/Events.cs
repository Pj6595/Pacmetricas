using UnityEngine;

namespace Pacmetricas_G01{
	public abstract class Event{
		public long gameSession;
		public double timeStamp;
		public string type;

		public Event(){
			gameSession = UnityEngine.Analytics.AnalyticsSessionInfo.sessionId;
			timeStamp =  Time.realtimeSinceStartupAsDouble;
		}

		public abstract string ToJSON();
		public abstract string ToCSV();

	}

	///////////////////////////////////////////////////////////////////////////////////////////
	//Eventos simples que solo cuentan con el tipo de evento, el timeStamp y el id de la sesión
	public class TimeStampEvent: Event{
		
		public override string ToJSON(){
			return JsonUtility.ToJson(this, true);
		}

		public override string ToCSV(){
			return type + "," + timeStamp + "," + gameSession + "\n";
		}
	}

	public class InitGameEvent: TimeStampEvent{
		
		public InitGameEvent(){
			type = "INIT_GAME";
		}
	}

	public class MenuPassedEvent: TimeStampEvent{
		
		public MenuPassedEvent(){
			type = "MENU_PASSED";
		}

	}

	public class FirstPhraseEvent: TimeStampEvent{
		
		public FirstPhraseEvent(){
			type = "FIRST_PHRASE";
		}
	}

	public class CorrectDirectionEvent: TimeStampEvent{
		
		public CorrectDirectionEvent(){
			type = "CORRECT_DIR";
		}
	}

	public class InitRunEvent: TimeStampEvent{
		
		public InitRunEvent(){
			type = "INIT_RUN";
		}
	}

	public class PlayerDeadEvent: TimeStampEvent{
		
		public PlayerDeadEvent(){
			type = "PLAYER_DEAD";
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////
	//Eventos que ademas incluyen frases que dice el jugador para intertar hacer alguna accion

	public class PhraseEvent: Event{
		public string phrase;
		
		public override string ToJSON(){
			return JsonUtility.ToJson(this, true);
		}

		public override string ToCSV(){
			return type + "," + timeStamp + "," + gameSession + "," + phrase + "\n";
		}
	}

	public class PhraseMenuEvent: PhraseEvent{
		
		public PhraseMenuEvent(){
			type = "TRY_PHRASE_MENU";
			phrase = "";
		}

		public PhraseMenuEvent(string tryphrase){
			type = "TRY_PHRASE_MENU";
			phrase = tryphrase;
		}
	}


	public class PhraseTaxiEvent: PhraseEvent{
		
		public PhraseTaxiEvent(){
			type = "TRY_PHRASE_TAXI";
			phrase = "";
		}

		public PhraseTaxiEvent(string tryphrase){
			type = "TRY_PHRASE_TAXI";
			phrase = tryphrase;
		}
	}

	public class PhraseBlackoutEvent: PhraseEvent{
		public PhraseBlackoutEvent(){
			type = "TRY_PHRASE_BLACKOUT";
			phrase = "";
		}

		public PhraseBlackoutEvent(string tryphrase){
			type = "TRY_PHRASE_BLACKOUT";
			phrase = tryphrase;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////
	//Estos eventos mandan ademas un valor de 0.0 a 1.0 que indica cierto parámetro del juego

	public class ValueEvent: Event{
		protected float value;
		
		public override string ToJSON(){
			return JsonUtility.ToJson(this, true);
		}

		public override string ToCSV(){
			return type + "," + timeStamp + "," + gameSession + "," + value  + "\n";
		}
	}

	public class MicrophoneVolume: ValueEvent{
		
		public MicrophoneVolume(){
			type = "VOLUME_MIC";
			value = -1.0f; //Usamos -1 como valor por defecto para saber si es erroneo
		}

		public MicrophoneVolume(float micValue){
			type = "VOLUME_MIC";
			value = micValue;
		}
	}

	public class BlackoutIntensityVolume: ValueEvent{
		
		public BlackoutIntensityVolume(){
			type = "BLACKOUT_INTENSITY";
			value = -1.0f; //Usamos -1 como valor por defecto para saber si es erroneo
		}

		public BlackoutIntensityVolume(float blackoutIntensity){
			type = "BLACKOUT_INTENSITY";
			value = blackoutIntensity;
		}
	}
}