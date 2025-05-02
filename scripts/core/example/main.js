export const run = () => {
  console.log(`Game Tick: ${Game.GetTick()}`);

  if (Object.keys(Game.GetBots()).length == 0) {
    const result = Game.CreateBot("keqing");
    console.log(`Bot Created: ${result}`);
    return;
  }

  const bots = Object.values(Game.GetBots());
  for (const bot of bots) {
    bot.Move(1);
  }
};
