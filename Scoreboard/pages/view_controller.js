import * as utilities from "./utilities.js";

export const controller = {
  template: `
<div class="mainmargin">
  <div v-if="loading" class="lds-ellipsis"><div></div><div></div><div></div><div></div></div>
  <div v-if="game" class="flexcolumn">    
    <div class="content row blue-background" style="max-width: 100vh; margin: 0 auto;">
        <div style="container-type: inline-size; width: 100%; height: 110vw; max-width: 100vh; max-height: 90vh;">
            <div style="display: grid; grid-template-columns: 1fr 1fr 1fr 1fr; grid-template-rows: 1fr 2fr 2fr 2fr 2fr 1fr; gap: 4px; padding: 4px; width: 100%; height: 100%; box-sizing: border-box;">
                <!-- Team Names -->
                <div class="section-label" style="grid-column: 1 / span 2; grid-row: 1; margin: 0; font-size: min(9cqw, 4.5cqh); position: relative;">
                    <span id="team-1-name" class="section-label-span">{{ game.team1 }}</span>
                </div>
                <div class="section-label" style="grid-column: 3 / span 2; grid-row: 1; margin: 0; font-size: min(9cqw, 4.5cqh); position: relative;">
                    <span id="team-2-name" class="section-label-span">{{ game.team2 }}</span>
                </div>

                <!-- Scores -->
                <div class="section-button" style="grid-column: 1 / span 2; grid-row: 2; margin: 0; position: relative;" @click="executeMethod('executeTeam1ScoreUp')">
                    <div class="icon-add" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh);"></div>
                </div>
                <div class="section-button" style="grid-column: 3 / span 2; grid-row: 2; margin: 0; position: relative;" @click="executeMethod('executeTeam2ScoreUp')">
                    <div class="icon-add" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh); position: absolute; right: 0;"></div>
                </div>
                <div class="section-button" style="grid-column: 1 / span 2; grid-row: 3; margin: 0; position: relative;" @click="executeMethod('executeTeam1ScoreDown')">
                    <div class="icon-remove" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh); position: absolute; bottom: 0;"></div>
                </div>
                <div class="section-button" style="grid-column: 3 / span 2; grid-row: 3; margin: 0; position: relative;" @click="executeMethod('executeTeam2ScoreDown')">
                    <div class="icon-remove" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh); position: absolute; right: 0; bottom: 0;"></div>
                </div>
                <div style="grid-column: 1 / span 2; grid-row: 2 / span 2; position: relative; pointer-events: none;">
                    <div id="team-1-score" class="score-large" style="font-size: min(50cqw, 30cqh)">{{ game.team1Score }}</div>
                </div>
                <div style="grid-column: 3 / span 2; grid-row: 2 / span 2; position: relative; pointer-events: none;">
                    <div id="team-2-score" class="score-large" style="font-size: min(50cqw, 30cqh)">{{ game.team2Score }}</div>
                </div>

                <!-- Shot Clock -->
                <div class="section-button" style="grid-column: 1; grid-row: 4; margin: 0; position: relative;" @click="executeMethod('executeShotClockReset')">
                    <div class="icon-reset" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh); position: absolute; left: 0; top: 0;"></div>
                </div>
                <div class="section-button" style="grid-column: 2; grid-row: 4 / span 2; margin: 0; position: relative;" @click="executeMethod('executeShotClockPlayPause')">
                    <div class="icon-play-pause" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh); position: absolute; right: 0; bottom: 0;"></div>
                </div>
                <div class="section-button" style="grid-column: 1; grid-row: 5; margin: 0; position: relative;" @click="executeMethod('executeShotClockResetResume')">
                    <div class="icon-reset-resume" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh); position: absolute; left: 0; bottom: 0;"></div>
                </div> 
                <div style="grid-column: 1 / span 2; grid-row: 4 / span 2; position: relative; pointer-events: none;">
                    <div id="shot-clock-time" class="shot-clock score-large" style="font-size: min(40cqw, 40cqh)">{{ game.shotClockDisplayTime }}</div>
                </div>
                <div class="section-button" style="grid-column: 1; grid-row: 6; margin: 0; position: relative;" @click="executeMethod('executeShotClockDecrement')">
                    <div class="icon-remove" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh); position: absolute; bottom: 0px; left: 0px;"></div>
                </div>
                <div class="section-button" style="grid-column: 2; grid-row: 6; margin: 0; position: relative;" @click="executeMethod('executeShotClockIncrement')">
                    <div class="icon-add" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh); position: absolute; bottom: 0px; right: 0px;"></div>
                </div>
                
                <!-- Main Clock -->
                <div class="section-button" style="grid-column: 3 / span 2; grid-row: 4 / span 3; position: relative;" @click="executeMethod('executePlayPause')">
                    <div id="period" style="position: absolute; width: 100%; height: 20%; top: 5%; font-size: min(9cqw, 4.5cqh); text-align: center; pointer-events: none;">{{ game.period }}</div>
                    <div class="icon-pause" style="width: min(6cqw, 3cqh); height: min(6cqw, 3cqh); position: absolute; right: 0; bottom: 0px;"></div>
                </div>
                <div style="grid-column: 3 / span 2; grid-row: 4 / span 3; position: relative; pointer-events: none;">
                    <div id="period-time" class="period-time" style="font-size: min(25cqw, 15cqh)">{{ game.mainTimeRemaining }}</div>
                </div>
            </div>
        </div>
    </div> 
  </div>
  <div v-if="!game && !loading">
    <h3>Error</h3>
    <p>There is no active game.</p>
  </div>
</div>
  `,
  data () {
    return {
      loading: false,
      game: undefined,
    }
  },
  created () {    
    this.loadData(false);
    this.webSocket = utilities.initialiseWebSocket(this.onWebSocketMessage, this.onCheckWebSocket);
  },
  mounted() {
    
  },
  methods:
  {    
    loadData: function() {
      this.getGame(this.$route.params.id)      
    },
    refresh: function() {
      this.loadData();      
    },
    onWebSocketMessage: function(e) {
      const game = JSON.parse(e.data);
      this.game = game; 
    },
    onCheckWebSocket: function() {
      this.webSocket = utilities.checkWebSocket(this.webSocket, this.onWebSocketMessage, this.onCheckWebSocket);
    },
    getGame: async function()
    {
      var _this = this;

      try {
        const response = await fetch ('/game');
        if (response?.ok)
        {
          const game = await response.json();      
          _this.game = game;                  
        }
        else
        {
          console.log(await response.text());
        }              
      } catch (error) {
        console.log(error);
      }
      _this.loading = false;
    },
    executeMethod: async function(method) {
      var _this = this;

      var xmlHttp = new XMLHttpRequest();
      xmlHttp.open("GET", method, true);
      xmlHttp.onload = function () {
          if (this.status >= 200 && this.status < 300) {
              _this.loadData();      
          }
      };
      xmlHttp.send();           
    }
  }   
};