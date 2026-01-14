import * as utilities from "./utilities.js";

export const controller = {
  template: `
<div class="mainmargin">
  <div v-if="loading" class="lds-ellipsis"><div></div><div></div><div></div><div></div></div>
  <div v-if="game" class="flexcolumn">    
    <div class="content row blue-background">
        <div style="width: 100%; height: 110vw; max-width: 100vh; max-height: 90vh;">
            <div class="section-padded" style="padding-right: 2px; width: 50%; height: 98.5%;">
                <div class="section-label" style="height: 10%;">
                    <span id="team-1-name" class="section-label-span">{{ game.team1 }}</span>
                </div>
                <div class="absolute-parent" style="height: 40%">                                    
                    <div class="section-button" style="height: 50%;" @click="executeMethod('executeTeam1ScoreUp')">
                        <div class="icon-add"></div>   
                    </div>
                    <div class="section-button" style="height: 50%;" @click="executeMethod('executeTeam1ScoreDown')">
                        <div class="icon-remove" style="position: absolute; bottom: 0;"></div>
                    </div>                                    
                    <div id="team-1-score" class="score-large">{{ game.team1Score }}</div>
                </div>                                
                <div class="absolute-parent" style="height: 40%; margin-top:8px;">                                    
                    <div style="width: 100%; height: 100%; position: absolute;">
                        <div class="section-button" style="width: 49%; height: 49%; float: left; margin-right: 4px" @click="executeMethod('executeShotClockReset')">
                          <div class="icon-reset" style="position: absolute; left: 0; top: 0;"></div>
                        </div>
                        <div class="section-button" style="width: 49%; height: 49%; float: left; margin-right: 4px" @click="executeMethod('executeShotClockResetResume')">
                          <div class="icon-reset-resume" style="position: absolute; left: 0; bottom: 0;"></div>          
                        </div>
                        <div class="section-button" style="width: auto; height: 100%; overflow:hidden; margin-left: 4px" @click="executeMethod('executeShotClockPlayPause')">                        
                          <div class="icon-play-pause" style="position: absolute; right: 0; bottom: 0;"></div>                        
                        </div>
                    </div>
                    <div id="shot-clock-time" class="shot-clock score-large">{{ game.shotClockDisplayTime }}</div>
                </div>
                <div class="absolute-parent" style="height: 9.4%; margin-top: 3px;">                                    
                    <div style="width: 100%; height: 100%; position: absolute;">
                        <div class="section-button" style="width: 49%; height: 100%; float: left; margin-right: 4px" @click="executeMethod('executeShotClockDecrement')">                        
                          <div class="icon-remove" style="position: absolute; left: 0; bottom: 15%;"></div>                      
                        </div>
                        <div class="section-button" style="width: auto; height: 100%; overflow:hidden; margin-left: 4px" @click="executeMethod('executeShotClockIncrement')">                        
                          <div class="icon-add" style="position: absolute; right: 0; bottom: 7%;"></div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="section-padded" style="padding-left: 2px; width: 50%; height: 98.5%;">
                <div class="section-label" style="height: 10%;">
                    <span id="team-2-name" class="section-label-span">{{ game.team2 }}</span>
                </div>
                <div class="absolute-parent" style="height: 40%">                                    
                    <div class="section-button" style="height: 50%;" @click="executeMethod('executeTeam2ScoreUp')">
                        <div class="icon-add" style="position: absolute; right: 0;"></div>
                    </div>
                    <div class="section-button" style="height: 50%;" @click="executeMethod('executeTeam2ScoreDown')">
                        <div class="icon-remove" style="position: absolute; right: 0; bottom: 0;"></div>
                    </div>                                    
                    <div id="team-2-score" class="score-large">{{ game.team2Score }}</div>
                </div>
                <div class="absolute-parent" style="height: 50%; margin-top:8px;">                                    
                    <div class="section-button" style="height: 100%;" @click="executeMethod('executePlayPause')">
                        <div id="period" style="position: absolute; width: 100%; height: 20%; top: 5%; font-size: 6vw; text-align: center; pointer-events: none;">{{ game.period }}</div>
                        <div class="icon-pause" style="position: absolute; right: 0; bottom: 0;"></div>                    
                    </div>                                                                            
                    <div id="period-time" class="period-time">{{ game.timeRemaining }}</div>
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