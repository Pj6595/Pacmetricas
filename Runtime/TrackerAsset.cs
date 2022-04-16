namespace Pacmetricas_G01
{
	public interface ITrackerAsset{
		public bool Accept(Event trackerEvent);
	}

	public class TrackerAsset: ITrackerAsset {
		public bool Accept(Event trackerEvent) {
            return true;
        }
	}

	// public class TrackerAsset: ITrackerAsset {
	// 	public bool Accept(Event trackerEvent) {

    //     }
	// }
}