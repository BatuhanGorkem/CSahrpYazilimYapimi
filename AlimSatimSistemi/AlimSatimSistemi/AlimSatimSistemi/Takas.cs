using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlimSatimSistemi
{
    class Takas
    {
        private static Talep SatisTalebiBul(Urun urun)
        {
            BorsavtDb db = new BorsavtDb();
            Talep aranan = null;
            foreach (Talep talep in db.Talepler.Where(x => x.TalepTuru == "Satış" && x.Urun.UrunAdi == urun.UrunAdi))
            {
                if (aranan == null)
                {
                    aranan = talep;
                }
                else
                {
                    if (talep.BirimFiyat > aranan.BirimFiyat)
                    {
                        aranan = talep;
                    }
                }
            }
            return aranan;
        }
        public static void TakaslariGerceklestir()
        {
            BorsavtDb db = new BorsavtDb();
            foreach (Talep talep in db.Talepler.Where(x => x.TalepTuru == "Alış" && x.Miktar>0))
            {
                Talep talepara = SatisTalebiBul(talep.Urun);
                Kullanici alici = talep.Kullanici;
                if (talepara != null&&alici.Bakiye>talepara.BirimFiyat)
                {
                    Kullanici satici = db.Kullanicilar.Find(talepara.Kullaniciadi);
                    KullaniciUrun alinanUrun = null;
                    int? alinacakMiktar;
                    if (talepara.Miktar >= talep.Miktar)
                    {
                        alinacakMiktar = talep.Miktar;
                    }
                    else
                    {
                        alinacakMiktar = talepara.Miktar;
                    }
                    int? toplamTutar = alinacakMiktar * talepara.BirimFiyat;
                    if (alici.Bakiye < toplamTutar)
                    {
                        alinacakMiktar = (int?)(alici.Bakiye / talepara.BirimFiyat);
                        toplamTutar = alinacakMiktar * talepara.BirimFiyat;
                    }
                    alinanUrun = new KullaniciUrun();
                    alinanUrun.Miktar = alinacakMiktar;
                    alinanUrun.Urunid = talepara.Urunid;
                    alinanUrun.KullaniciAdi = alici.KullaniciAdi;
                    alinanUrun.Onay = 1;
                    KullaniciUrun kullanicininUrunu = alici.KullaniciUrunleri.Where(x => x.Urunid == talepara.Urunid).FirstOrDefault();
                    if (kullanicininUrunu!=null)
                    {
                        kullanicininUrunu.Miktar += alinacakMiktar;
                    }
                    else
                    {
                        alici.KullaniciUrunleri.Add(alinanUrun);
                    }
                    alici.Bakiye -= toplamTutar;
                    satici.Bakiye += toplamTutar;
                    talepara.Miktar -= alinacakMiktar;
                    talep.Miktar -= alinacakMiktar;
                    Islem islem = new Islem();
                    islem.IslemZamani = DateTime.Now;
                    islem.Detay = alici.Ad + " " + alinacakMiktar + " kilo " + talep.Urun.UrunAdi + " almak ister ise " + talepara.BirimFiyat + " tl'den alım işlemi gerçekleşti.";
                    islem.Tutar = alici.Ad + " " + satici.Ad + "'ın hesabına " + toplamTutar + " tl para gönderdi.";
                    islem.KalanTutar = alici.Ad + " " + alici.Bakiye + " tl parası kaldı.";
                    islem.BirimFiyat = talepara.BirimFiyat + " tl";
                    db.Islemler.Add(islem);
                }
            }
            db.SaveChanges();
            List<Talep> talepler = db.Talepler.Where(x => x.Miktar == 0).ToList();
            foreach (Talep silinecekTalep in talepler)
            {
                db.Talepler.Remove(db.Talepler.Find(silinecekTalep.Id));
            }
            List<KullaniciUrun> silinecekUrunler = db.KullaniciUrunleri.Where(x => x.Miktar == 0 && x.Onay == 1).ToList();
            foreach (KullaniciUrun silinecek in silinecekUrunler)
            {
                db.KullaniciUrunleri.Remove(db.KullaniciUrunleri.Find(silinecek.Id));
            }
            db.SaveChanges();
        }
    }
}
