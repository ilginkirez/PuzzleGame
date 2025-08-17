# 🎮 Oyna  
👉 [Tarayıcıda Oyna (Itch.io)](https://ilginkirez.itch.io/unpuzzle)  

---

# 🧩 Unpuzzle - Puzzle Project  

## 🎮 Oyun Hakkında  
**Unpuzzle**, oyuncunun sınırlı hamlelerle küpleri doğru yönlere hareket ettirerek sahneyi boşaltmaya çalıştığı bir **3D puzzle oyunu**dur.  

- **Amaç:** Tüm küpleri sahneden kaldırmak  
- **Kısıt:** Belirli sayıda hamle hakkı  
- **Özel Küpler:** İleride farklı tipler (Special, Bomb, Multiplier) desteklenecek  

---

## 🚀 Özellikler  
- 📦 **Object Pooling** ile performanslı prefab yönetimi  
- 🎛️ **Grid tabanlı sistem** (küp yerleşimi ve hareket kontrolü)  
- 🎬 **DOTween animasyonları** ile UI efektleri  
- 🖱️ **Mouse & Touch Input Handler** (hem PC hem mobil uyumlu)  
- 🎵 **Ses efektleri** (tıklama, hareket, engellenme)  
- 🗂️ **JSON tabanlı level tanımları** (`Resources/Levels`)  
- 🧩 **Level başarı & başarısızlık panelleri**  

---

## 🛠️ Teknolojiler  
- Unity **2022+**  
- C# (modern OOP + design patterns)  
- DOTween (animasyonlar)  
- TextMeshPro (UI yazıları)  
- Unity Profiler (performans optimizasyonu)  

---

## 📖 Kullanılan Design Patterns  
- **Singleton** → Manager sınıflarında (GameManager, PoolManager vb.)  
- **Factory** → CubeFactory (küplerin üretimi)  
- **Object Pooling** → PoolManager (performans optimizasyonu)  
- **Observer / Event-driven** → Manager’lar arası haberleşme  
- **Handler Pattern** → Input yönetimi (mouse & touch)  

---

## 🎮 Nasıl Oynanır?  
1. Oyunu başlat → **Ana Menü** açılır  
2. **Play** tuşuna bas  
3. Küplere tıkla → Belirlenen yönde hareket eder  
4. Eğer tüm küpler kaldırılırsa → **Başarı (Success) ekranı** gösterilir  
5. Hamle hakkın biterse → **Başarısızlık (Failure) ekranı** gösterilir  
