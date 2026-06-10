import asyncio
import threading
import json
import falcon
import waitress
import websockets

STATE = {}
CLIENTS = set()

HTML_PAGE = r"""<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>Proto Display Emulator</title>
<style>
body { background: #111; color: #fff; font-family: 'Segoe UI', monospace; margin: 0; padding: 20px; }
h2 { text-align: center; color: #888; font-size: 14px; margin: 0 0 10px 0; }
.grid {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    grid-template-rows: auto auto auto;
    gap: 2px;
    max-width: 800px;
    margin: 0 auto;
    background: #000;
    border: 2px solid #333;
}
.cell {
    padding: 20px 10px;
    text-align: center;
    font-size: 36px;
    font-weight: bold;
    min-height: 60px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid #222;
}
.cell.name { font-size: 28px; }
.cell.period { font-size: 24px; }
.cell.score { font-size: 48px; }
.cell.time { font-size: 48px; }
.cell.shot { font-size: 36px; }
.cell.empty { background: #000; }
#NameTeamA { color: #ffffff; }
#PeriodName { color: #ffffff; }
#NameTeamB { color: #ffffff; }
#ScoreA { color: #88ff88; }
#TimeDisplay { color: #ffffff; }
#ScoreB { color: #88ff88; }
#Shotclock { color: #ff8888; }
.log {
    max-width: 800px;
    margin: 20px auto 0;
    background: #1a1a1a;
    border: 1px solid #333;
    padding: 10px;
    max-height: 200px;
    overflow-y: auto;
    font-family: monospace;
    font-size: 11px;
    color: #aaa;
}
.log div { padding: 1px 0; border-bottom: 1px solid #222; }
.log .new { color: #8f8; }
</style>
</head>
<body>
<h2>Proto Display Emulator</h2>
<div class="grid">
    <div class="cell name" id="NameTeamA"></div>
    <div class="cell period" id="PeriodName"></div>
    <div class="cell name" id="NameTeamB"></div>

    <div class="cell score" id="ScoreA"></div>
    <div class="cell time" id="TimeDisplay">
        <span id="Minutes">00</span><span id="ColonS" style="opacity:1">:</span><span id="Seconds">00</span>
    </div>
    <div class="cell score" id="ScoreB"></div>

    <div class="cell empty"></div>
    <div class="cell shot" id="Shotclock">00</div>
    <div class="cell empty"></div>
</div>
<div class="log" id="log"></div>
<script>
const ws = new WebSocket('ws://localhost:9090');
ws.onmessage = function(event) {
    const msg = event.data;
    addLog(msg);
    parseCommand(msg);
};
ws.onopen = function() { addLog('Connected', true); };
ws.onclose = function() { addLog('Disconnected', true); };
ws.onerror = function() { addLog('Error', true); };

function addLog(msg, highlight) {
    const div = document.createElement('div');
    div.textContent = new Date().toLocaleTimeString() + ' ' + msg;
    if (highlight) div.className = 'new';
    const log = document.getElementById('log');
    log.appendChild(div);
    log.scrollTop = log.scrollHeight;
}

function parseCommand(cmd) {
    if (!cmd.startsWith('SLAVE,')) return;
    if (cmd === 'SLAVE,Siren,true,true,CS') {
        addLog('** SIREN ON **', true);
        return;
    }
    if (cmd === 'SLAVE,Siren,false,false,CS') {
        addLog('** SIREN OFF **', true);
        return;
    }
    const parts = cmd.split(',');
    if (parts.length < 3) return;
    const action = parts[1];
    const target = parts[2];
    const value = parts[3];
    const suffix = parts[4];
    if (suffix !== 'CS') return;

    if (action === 'sendText') {
        if (target === 'NameTeamA') { document.getElementById('NameTeamA').textContent = value; }
        else if (target === 'NameTeamB') { document.getElementById('NameTeamB').textContent = value; }
        else if (target === 'ScoreA') { document.getElementById('ScoreA').textContent = value; }
        else if (target === 'ScoreB') { document.getElementById('ScoreB').textContent = value; }
        else if (target === 'Minutes') { document.getElementById('Minutes').textContent = value; }
        else if (target === 'Seconds') { document.getElementById('Seconds').textContent = value; }
        else if (target === 'Shotclock') { document.getElementById('Shotclock').textContent = value; }
        else if (target === 'intShotclock') { }
        else if (target === 'PeriodName') { document.getElementById('PeriodName').textContent = value; }
    } else if (action === 'configText') {
        var el = null;
        if (target === 'NameTeamA') el = document.getElementById('NameTeamA');
        else if (target === 'NameTeamB') el = document.getElementById('NameTeamB');
        else if (target === 'ScoreA') el = document.getElementById('ScoreA');
        else if (target === 'ScoreB') el = document.getElementById('ScoreB');
        else if (target === 'Minutes') el = document.getElementById('Minutes');
        else if (target === 'ColonS') el = document.getElementById('ColonS');
        else if (target === 'Seconds') el = document.getElementById('Seconds');
        else if (target === 'Shotclock') el = document.getElementById('Shotclock');
        else if (target === 'intShotclock') { }
        if (el) el.style.color = value;
    } else if (action === 'setText') {
        if (target === 'PeriodName') {
            document.getElementById('PeriodName').style.display = value === 'true' ? '' : 'none';
        }
    }
}
</script>
</body>
</html>
"""

class StaticResource:
    def on_get(self, req, resp):
        resp.content_type = falcon.MEDIA_HTML
        resp.text = HTML_PAGE

class ProtoDisplay:
    def on_get(self, req, resp):
        resp.media = STATE
        resp.content_type = falcon.MEDIA_JSON

app = falcon.App()
app.add_route('/', StaticResource())
app.add_route('/state', ProtoDisplay())

async def ws_handler(websocket):
    CLIENTS.add(websocket)
    try:
        async for message in websocket:
            parse_and_update(message)
            for client in CLIENTS:
                if client != websocket:
                    try:
                        await client.send(message)
                    except:
                        pass
    except websockets.exceptions.ConnectionClosed:
        pass
    finally:
        CLIENTS.discard(websocket)

def parse_and_update(cmd):
    if not cmd.startswith('SLAVE,'):
        return
    if cmd == 'SLAVE,Siren,true,true,CS':
        STATE['Siren'] = True
        return
    if cmd == 'SLAVE,Siren,false,false,CS':
        STATE['Siren'] = False
        return
    parts = cmd.split(',')
    if len(parts) < 3:
        return
    action = parts[1]
    rest = ','.join(parts[2:])
    eq_idx = rest.find(',')
    if eq_idx == -1:
        return
    target = rest[:eq_idx]
    value = rest[eq_idx + 1:]
    if value != 'CS':
        return
    if action == 'sendText':
        STATE[target] = STATE.get(target, {})
        STATE[target]['text'] = rest[eq_idx + 1:].replace(',CS', '') if ',CS' in rest[eq_idx:] else value
    elif action == 'configText':
        STATE[target] = STATE.get(target, {})
        STATE[target]['color'] = value.rstrip(',CS') if value.endswith(',CS') else value
    elif action == 'setText':
        STATE[target] = STATE.get(target, {})
        STATE[target]['visible'] = value.rstrip(',CS') if value.endswith(',CS') else value
    print(f"  {action} {target} = {value}")

def run_ws_server():
    async def serve():
        print("WebSocket server on ws://localhost:9090")
        async with websockets.serve(ws_handler, "0.0.0.0", 9090):
            await asyncio.Future()

    asyncio.run(serve())

def run_http_server():
    print("HTTP server on http://localhost:8081")
    waitress.serve(app, host="0.0.0.0", port=8081)

if __name__ == '__main__':
    print("Proto Display Emulator")
    print("  Open http://localhost:8081 in a browser")
    print("  Connect Scoreboard Proto to ws://localhost:9090")
    print()

    ws_thread = threading.Thread(target=run_ws_server, daemon=True)
    ws_thread.start()

    run_http_server()
