export const game = {
  template: `
<div class="nomargin">
  <div v-if="loading" class="lds-ellipsis"><div></div><div></div><div></div><div></div></div>
  <div v-if="game" class="flexcolumn">    
    <div>Team 1: {{ game.team1 }}: {{ game.team1score }}</div>
    <div>Team 2 {{ game.team2 }}: {{ game.team2score }}</div>    
    <div>Time: {{ game.timeRemaining }} {{ game.period }}</div> 
  </div>
  <div v-if="!game && !loading">
    <p>Oops. Something went wrong.</p>  
    <router-link to="/tournaments">Tournaments</router-link>
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