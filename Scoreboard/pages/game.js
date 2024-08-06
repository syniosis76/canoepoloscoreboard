import * as utilities from "./utilities.js";

export const game = {
  template: `
<div class="mainmargin">
  <div v-if="loading" class="lds-ellipsis"><div></div><div></div><div></div><div></div></div>
  <div v-if="game" class="flexcolumn">    
    <div class="flexrow">
      <div class="card gamecell"><h2>{{ game.team1 }}</h2></div>
      <div class="card gamecell"><h2>{{ game.period }}</h2></div>
      <div class="card gamecell"><h2>{{ game.team2 }}</h2></div>
    </div>
    <div class="flexrow">
      <div class="card gamecell gamenumber"><h2>{{ game.team1Score }}</h2></div>
      <div class="flexcolumn">
        <div class="card gamecell gamenumber"><h2>{{ game.timeRemaining }}</h2></div>
        <div class="card gamecell gamenumber"><h2>{{ game.shotClockTime }}</h2></div>
      </div>
      <div class="card gamecell gamenumber"><h2>{{ game.team2Score }}</h2></div>
    </div>
  </div>
  <div v-if="!game && !loading">
    <p>Oops. Something went wrong.</p>  
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