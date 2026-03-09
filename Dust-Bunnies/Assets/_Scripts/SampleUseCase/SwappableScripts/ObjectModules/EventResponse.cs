public class EventResponse { public bool received = false; }
public class EventResponse<T> : EventResponse {
    public T objectReference;
}