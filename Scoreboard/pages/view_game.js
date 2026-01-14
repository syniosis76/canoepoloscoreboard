import * as utilities from "./utilities.js";

export const game = {
  template: `
<div class="mainmargin">
  <div v-if="loading" class="lds-ellipsis"><div></div><div></div><div></div><div></div></div>
  <div v-if="game" class="flexcolumn">    
    <div class="flexrow">
      <div class="card gamecell">{{ game.team1 }}</div>
      <div class="card gamecell">{{ game.period }}</div>
      <div class="card gamecell">{{ game.team2 }}</div>
    </div>
    <div class="flexrow">
      <div class="card gamecell gamenumber" style="font-size: 70pt">{{ game.team1Score }}</div>
      <div class="flexcolumn">
        <div class="card gamecell gamenumber">{{ game.timeRemaining }}</div>
        <div class="card gamecell gamenumber">{{ game.shotClockDisplayTime }}</div>
      </div>
      <div class="card gamecell gamenumber" style="font-size: 70pt">{{ game.team2Score }}</div>
    </div>
    <p>
      <div class="flexcolumn" style="font-size: 20pt">
        <div class="flexrow">
          <a href="/scoreboard.html">Scoreboard</a>
        </div>  
        <div class="flexrow">
          <a href="/shot-clock.html">Shot Clock</a>
        </div>
        <div class="flexrow">
          <RouterLink to="/controller">Controller</RouterLink>
        </div>
        <div class="flexrow">
          <RouterLink to="/shotcontroller">Shot Clock Controller</RouterLink>
        </div>        
      </div>
    </p>
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
      _this.loading = true;
      _this.game = null;

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
    }
  }   
};