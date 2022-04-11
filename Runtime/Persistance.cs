namespace Pacmetricas_G01{
    
    public abstract class IPersistance{

        protected ISerializer serializer;
        public abstract void SendEvent(Event trackerEvent);
        public abstract void Flush();
        public ISerializer GetSerializer(){return serializer;}
    }

    public class FilePersistance: IPersistance{
        
        public FilePersistance(ISerializer currSerializer){
            serializer = currSerializer;
        }

        public override void SendEvent(Event trackerEvent){

        }
        
        public override void Flush(){

        }
    }

    public class ServerPersistance: IPersistance{

        //Server ¿?¿?¿ 
        public ServerPersistance(ISerializer currSerializer){
            serializer = currSerializer;
        }

        public override void SendEvent(Event trackerEvent){

        }
        public override void Flush(){
            
        }
    }
}