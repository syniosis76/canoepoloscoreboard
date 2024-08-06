export function initialiseWebSocket(onWebSocketMessage, onCheckWebSocket) {
  const webSocketUri = "ws://" + window.location.host;
  const webSocket = new WebSocket(webSocketUri);

  webSocket.onmessage = (e) => {
    onWebSocketMessage(e);
  };

  webSocket.onerror = (e) => {
    // Todo?
  };

  // Set Timer to check the web socket every 5 seconds and recreate if needed.
  setInterval(onCheckWebSocket, 5000);

  return webSocket;
}

export function checkWebSocket(webSocket, onWebSocketMessage, onCheckWebSocket) {  
  if (webSocket.readyState !== WebSocket.CLOSED) {
    return webSocket;
  }
  else {
    return initialiseWebSocket(onWebSocketMessage, onCheckWebSocket);
  }
}