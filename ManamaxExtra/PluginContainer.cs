using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using System.IO;

namespace ManamaxExtra
{
    [ApiVersion(2, 1)]
    public class ManaMaxExtra : TerrariaPlugin
    {
        public override string Name => "ManaMaxExtra";
        public override string Author => "YourName";
        public override string Description => "Menaikkan maksimum mana di atas 200 menggunakan mana star (item ID 109).";
        public override Version Version => new Version(1, 0, 0);

        public static Configuration Config;
        private bool[] controlUseItemOld;
        private int[] itemUseTime;

        public ManaMaxExtra(Main game) : base(game)
        {
            LoadConfig();
            controlUseItemOld = new bool[255];
            itemUseTime = new int[255];
        }

        private static void LoadConfig()
        {
            Config = Configuration.Read(Configuration.FilePath);
            Config.Write(Configuration.FilePath);
        }

        private static void ReloadConfig(ReloadEventArgs args)
        {
            LoadConfig();
            args.Player?.SendSuccessMessage("[ManaMaxExtra] Konfigurasi telah dimuat ulang.");
        }

        public override void Initialize()
        {
            GeneralHooks.ReloadEvent += ReloadConfig;
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= ReloadConfig;
                PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
            }
            base.Dispose(disposing);
        }

        private void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            foreach (TSPlayer tsplayer in TShock.Players)
            {
                if (tsplayer != null)
                {
                    CheckAndSetPlayerMana(tsplayer);
                }
            }
        }

        private static void CheckAndSetPlayerMana(TSPlayer tsplayer)
        {
            int index = tsplayer.Index;
            Player tplayer = tsplayer.TPlayer;

            // Jika statManaMax2 melebihi nilai yang diizinkan, set ke CustomManaMax.
            if (tplayer.statManaMax2 > Config.CustomManaMax)
            {
                tplayer.statManaMax2 = Config.CustomManaMax;
                tsplayer.SendData(PacketTypes.PlayerMana, "", index);
            }
        }

        private void OnUpdate(EventArgs args)
        {
            foreach (TSPlayer tsplayer in TShock.Players)
            {
                if (tsplayer != null)
                {
                    int index = tsplayer.Index;
                    Player tplayer = tsplayer.TPlayer;
                    Item heldItem = tplayer.HeldItem;

                    // Jika pemain mulai menggunakan item dan waktu penggunaan telah habis
                    if (!controlUseItemOld[index] && tplayer.controlUseItem && itemUseTime[index] <= 0)
                    {
                        int type = heldItem.type;

                        // Cek jika item yang digunakan adalah mana star (item ID 109)
                        if (type == 109)
                        {
                            // Jika statManaMax2 masih di bawah batas CustomManaMax
                            if (tplayer.statManaMax2 < Config.CustomManaMax)
                            {
                                // Kurangi jumlah item di inventory
                                tplayer.inventory[tplayer.selectedItem].stack--;
                                tsplayer.SendData(PacketTypes.PlayerSlot, "", index, (float)tplayer.selectedItem);

                                // Tambahkan stat mana maksimum sebanyak 20
                                tplayer.statManaMax2 += 20;

                                // Kirim update ke klien agar tampilan mana diperbarui
                                tsplayer.SendData(PacketTypes.PlayerMana, "", index);
                            }
                            // Jika mana melebihi batas, set ke nilai CustomManaMax
                            else if (tplayer.statManaMax2 > Config.CustomManaMax)
                            {
                                tplayer.statManaMax2 = Config.CustomManaMax;
                                tsplayer.SendData(PacketTypes.PlayerMana, "", index);
                            }
                        }
                    }
                    controlUseItemOld[index] = tplayer.controlUseItem;
                }
            }
        }
    }
}
