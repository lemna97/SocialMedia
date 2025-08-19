using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volvo.Release
{
    public static class PromptConfig
    {
        /// <summary>
        /// 简介，五点描述
        /// </summary>
        public static string promptTitleDesc = "我会给你一个产品的图片和产品标题，请你以我提供的产品标题和产品的图片作为参考，帮我的商品写简介和五点描述，风格接近于沃尔玛中的产品，在标题中埋入尽可能多的有效关键词，返回简介、五点描述给我，并以英文json的格式返回给我，严格按照要求：简介在500到1000字符之间不超过1000字符。例:{'description':'nice cap','keyFeatures':[\"nice cap\",\"good cap\"]},你只需要将json返回给我，不需要说其他多余的文字，不需要有多余的提示，不需要有多余的表情符号。注意：所有填写或选择的属性值都必须使用英文，不要使用中文或其他语言";

        /// <summary>
        /// 项链属性识别
        /// </summary>
        public static string promptNecklaceAttributes = @"我会给你一个项链产品的图片和产品标题，请你分析图片和文案，帮我识别以下属性并严格按照要求选择：

1. claspType（扣环类型）- 从以下选项中选择最合适的，如果识别不到则填Lobster Claw：
Ball, Butterfly Clasp, Push Clasp, Swivel Clasp, Pop-Style Clasp, Multi-Strand Clasp, Hook & Eye Clasp, Box Clasp, Twister Clasp, Fold Over, Spring Ring, Toggle, Lobster Claw, Double Locking Fold-Over, Hinged Clip Clasp, Slide Lock Clasp, Double Safety, Multi-Strand Box, Buckle, Barrel Clasp, Magnetic Clasp, Button Clasp, Crimping Clasp, Filigree Clasp, Fishhook, S Hook, Seamless, No Clasp

2. necklaceStyle（项链风格）- 从以下选项中选择最合适的：
Locket, Chain, Lariat, Solitaire, Statement, Choker, Y-Necklace, Name, Sentiment & Symbols, Station, Riviera, Pendant, Beaded, Multi-Strand, Bib, Initial, Collar, Single Strand

3. chainLength（链子长度）- 提取数值并转换为英寸inch单位，如果识别不到则填18

4. chainPattern（链子图案）- 从以下选项中选择最合适的：
Ball, Paperclip, Rolo, Cable Wire, Figaro, Beveled Herringbone, Lace, Coreana, Cuban Link, Hollow Mariner, Cord, Franco, Sparkle, Serpentine, Byzantine, Mesh, Box, Open Cable, Rope, Bismark, Round Omega, Crisscross, Wheat, Link, San Marco, Cable, Singapore, Double Cable, Triple Rope, Flat Figaro, Omega, French Rope, Parallel Curb, Bead, Palma, Cascade, Cobra, Herringbone, Curb, Elongated Cable, Hexagon, Oval Curb, Snake, Boston Link, Popcorn, Mariner, Diamond Cut Figaro, Milano, Belcher, Anchor, Bar, Twister Serpentine, Twister Rope, C-Chain, Double Curb, Draw Flat Cable

5. chainWidthSize（链子宽度）- 提取数值并转换为英寸inch单位，如果识别不到则填0.059

6. dropLength（吊坠长度）- 提取数值并转换为英寸inch单位，如果识别不到则填1

严格按照JSON格式返回：{""claspType"":""选择的扣环类型"",""necklaceStyle"":""选择的项链风格"",""chainLength"":数值,""chainPattern"":""选择的链子图案"",""chainWidthSize"":数值,""dropLength"":数值}

你只需要将json返回给我，不需要说其他多余的文字，不需要有多余的提示，不需要有多余的表情符号。注意：所有填写或选择的属性值都必须使用英文，不要使用中文或其他语言";

        /// <summary>
        /// 耳饰属性识别
        /// </summary>
        public static string promptEarringsAttributes = @"我会给你一个耳饰产品的图片和产品标题，请你分析图片和文案，帮我识别以下属性并严格按照要求选择：

1. earringBack（耳饰背扣）- 从以下选项中选择最合适的：
Endless, Hinge, Lever - Back, Ear - Wire, La Pousette, Screw - Back, Ball Back, Kidney Wire, Magnetic, Clip-On, Friction, Omega-Back, Latch, Stick-On, Safety, La-Pousette-Posts-And-Clutches

2. earringStyle（耳饰风格）- 从以下选项中选择最合适的，如果识别不到则填Stud：
Jacket, Teardrop, C-Hoop, Climber, Threader, Hoop, Drop, Bajoran, Cuff, Chandelier, Stud, Huggie, Tassel, Stick-On, Dangle, Mismatch

3. earringFeature（耳饰特性）- 固定值：Sensitive Ear

严格按照JSON格式返回：{""earringBack"":""选择的耳饰背扣"",""earringStyle"":""选择的耳饰风格"",""earringFeature"":""Sensitive Ear""}

你只需要将json返回给我，不需要说其他多余的文字，不需要有多余的提示，不需要有多余的表情符号。注意：所有填写或选择的属性值都必须使用英文，不要使用中文或其他语言";

        /// <summary>
        /// 手链属性识别
        /// </summary>
        public static string promptBraceletsAttributes = @"我会给你一个手链产品的图片和产品标题，请你分析图片和文案，帮我识别以下属性并严格按照要求选择：

1. braceletStyle（手链风格）- 从以下选项中选择最合适的，如果识别不到则填Charm：
Tennis, Chain, Wrap, Stretch, Bolo, Slap, Friendship, Cuff, Charm, Beaded, Multi-Strand, Bangle, Cord, Link

2. claspType（扣环类型）- 从以下选项中选择最合适的，如果识别不到则填Lobster Claw：
Ball, Butterfly Clasp, Push Clasp, Swivel Clasp, Pop-Style Clasp, Multi-Strand Clasp, Hook & Eye Clasp, Box Clasp, Twister Clasp, Fold Over, Spring Ring, Toggle, Lobster Claw, Double Locking Fold-Over, Hinged Clip Clasp, Slide Lock Clasp, Double Safety, Multi-Strand Box, Buckle, Barrel Clasp, Magnetic Clasp, Button Clasp, Crimping Clasp, Filigree Clasp, Fishhook, S Hook, Seamless, No Clasp

3. chainLength（链子长度）- 提取数值并转换为英寸inch单位，如果识别不到则填7

4. chainPattern（链子图案）- 从以下选项中选择最合适的：
Ball, Paperclip, Rolo, Cable Wire, Figaro, Beveled Herringbone, Lace, Coreana, Cuban Link, Hollow Mariner, Cord, Franco, Sparkle, Serpentine, Byzantine, Mesh, Box, Open Cable, Rope, Bismark, Round Omega, Crisscross, Wheat, Link, San Marco, Cable, Singapore, Double Cable, Triple Rope, Flat Figaro, Omega, French Rope, Parallel Curb, Bead, Palma, Cascade, Cobra, Herringbone, Curb, Elongated Cable, Hexagon, Oval Curb, Snake, Boston Link, Popcorn, Mariner, Diamond Cut Figaro, Milano, Belcher, Anchor, Bar, Twister Serpentine, Twister Rope, C-Chain, Double Curb, Draw Flat Cable

5. chainWidthSize（链子宽度）- 提取数值并转换为英寸inch单位，如果识别不到则填0.059

严格按照JSON格式返回：{""braceletStyle"":""选择的手链风格"",""claspType"":""选择的扣环类型"",""chainLength"":数值,""chainPattern"":""选择的链子图案"",""chainWidthSize"":数值}

你只需要将json返回给我，不需要说其他多余的文字，不需要有多余的提示，不需要有多余的表情符号";

        /// <summary>
        /// 戒指属性识别
        /// </summary>
        public static string promptRingsAttributes = @"我会给你一个戒指产品的图片和产品标题，请你分析图片和文案，帮我识别以下属性并严格按照要求选择：

1. isRingResizable（戒指是否可调节）- 固定值：No

2. ringStyle（戒指风格）- 从以下选项中选择最合适的，如果识别不到则填Solitaire：
Chain, Wrap, Stacking, Solitaire, Spinner, Cocktail, Band, Delicate, Knot, Nugget, Claddagh, Open, Multi - Stone, Signet, Charm, Military, Dome, Composite, Infinity, Midi, Statement, Anniversary, Cluster, Eternity, Filigree, Promise, Puzzle, Mood, Semi-Eternity, Bypass, Beaded, Crossover

3. ringSize（戒指尺寸）- 从以下选项中选择最合适的，如果识别不到则填7：
3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10, 10.5, 11, 11.5, 12, 12.5, 13, 13.5, 14, 14.5, 15

严格按照JSON格式返回：{""isRingResizable"":""No"",""ringStyle"":""选择的戒指风格"",""ringSize"":""选择的戒指尺寸""}

你只需要将json返回给我，不需要说其他多余的文字，不需要有多余的提示，不需要有多余的表情符号。注意：所有填写或选择的属性值都必须使用英文，不要使用中文或其他语言";

        /// <summary>
        /// 通用属性识别
        /// </summary>
        public static string promptCommonAttributes = @"我会给你一个珠宝产品的图片、产品标题和五点描述，请你分析图片和文案，帮我识别以下属性并严格按照要求选择：

1. color（颜色）- 从以下选项中选择最合适的，如果识别不到则填Silver：
Silver, Yellow Gold, White Gold, Rose Gold

2. colorCategory（颜色类别）- 从以下选项中选择最合适的，如果识别不到则填Gold：
Brown, Black, Orange, Clear, Beige, Off-White, Bronze, Gold, Gray, Blue, Multicolor, Red, Silver, White, Pink, Yellow, Purple, Green

3. gemstoneType（宝石类型）- 从以下选项中选择最合适的，如果识别不到则填Cubic Zirconia：
Saltwater Pearl, Alexandrite, Amber, Ceylon Sapphire, Black Star Sapphire, Nephrite, Amazonite, Zircon, Prasiolite, Feldspar, White Opal, Beryl, Aquamarine, Iolite, Prehnite, Pink Opal, Rhodolite, Boulder Opal, Apple Quartz, Ammolite, Melo, Hematite, Spectrolite, Sodalite, Pink Sapphire, Thulite, Rainbow Moonstone, Labradorite, Cat's Eye, Mother-of-Pearl, Howlite, Moss Agate, Diamond, Rhodochrosite, Nano, Corundum, Blue Topaz, Coral, Pyrope, Chrysoberyl, Topaz, Hyalite, Jade, Borate, Morganite, Black Coral, Chrysolite, Jadeite, Flint, Agate, Cordierite, White Sapphire, Chalcedony, Goshenite, Garnet, Keshi Pearl, Cubic Zirconia, Sapphire, Spessartine, Turquoise, Akoya Cultured Pearl, Kunzite, Quartz, Carnelian, Lapis Lazuli, Smoky Quartz, Other Stone, Matrix Opal, Emerald, Citrine, Mystic Quartz, Ruby, Indicolite, Aventurine, Peruvian Opal, Blue Coral, Jasper, Moissanite, Spinel, Baltic amber, Green Sapphire, Rubellite, Almandine, Abalone, Ametrine, Cultured Diamond, Amethyst, Mystic Topaz, Red Coral, Rock Crystal, Padparadscha Sapphire, Heliodor, Conch, Apophyllite, Jelly Opal, Chrome Diopside, Bloodstone, Tanzanite, Apatite, White Topaz, Rhodonite, Tourmaline, Zoisite, Fire Opal, Moonstone, Pearl, Fossilized Coral, No Stone, Rose-de-France, Diopside, Tahitian Pearl, Paraiba, Chrysoprase, Pink Coral, Imperial Topaz, South Sea Pearl, Onyx, Tiger's Eye, Peridot, Freshwater Cultured Pearls, Malachite, Crystal, Sponge Coral, Rose Quartz, Sunstone, Orange Sapphire, Tsavorite, White Coral, Opal

4. material（材质）- 从以下选项中选择最合适的，如果识别不到则填Silver：
Silver, Copper

5. metalType（金属类型）- 从以下选项中选择最合适的，如果识别不到则填Sterling Silver：
Silver - Plated, Black Gold, Aluminum, Sterling Silver, Brass, Silvertone, Two - Tone Gold, Platinaire, White Gold - Plated, Stainless Steel, Platinum, Pewter, Tungsten, Titanium, White Gold, Chrome, Copper, Yellow Gold - Plated, Tri-Tone Gold, No Metal, Bronze, Gold, Rose Gold, Palladium, Steel, Yellow Gold, Gold-Plated, Silver, Black Gold - Plated, Rose Gold - Plated, Rhodium, Gunmetal, Goldtone, Tin, Nickel

6. netContentMeasure（净重量数值）- 提取产品重量数值，如果识别不到则填2.8

7. netContentUnit（净重量单位）- 如果识别不到则填Gram

8. assembledProductDepthMeasure（产品深度数值）- 提取产品长度尺寸数值，如果识别不到则填null

9. assembledProductDepthUnit（产品深度单位）- 从以下选项中选择：in, mm，如果识别不到则填null
注意：assembledProductDepthMeasure和assembledProductDepthUnit必须同时填写或同时为null

10. assembledProductHeightMeasure（产品高度数值）- 提取产品高度尺寸数值，如果识别不到则填null

11. assembledProductHeightUnit（产品高度单位）- 从以下选项中选择：in, mm，如果识别不到则填null

12. assembledProductWeightMeasure（产品重量数值）- 提取产品重量数值，如果识别不到则填null

13. assembledProductWeightUnit（产品重量单位）- 从以下选项中选择：lb, g, oz，如果识别不到则填null
注意：assembledProductWeightMeasure和assembledProductWeightUnit必须同时填写或同时为null

14. assembledProductWidthMeasure（产品宽度数值）- 提取产品宽度尺寸数值，如果识别不到则填null

15. assembledProductWidthUnit（产品宽度单位）- 从以下选项中选择：in, mm，如果识别不到则填null

16. birthstoneMonth（生辰石月份）- 从以下选项中选择最合适的，如果识别不到则填April - Diamond：
July - Ruby, December - Tanzanite, Zircon, Turquoise, November - Topaz, Citrine, June - Pearl, Alexandrite, May - Emerald, September - Sapphire, March - Aquamarine, October - Tourmaline, Opal, April - Diamond, February - Amethyst, August - Peridot, January - Garnet

17. itemsIncluded（包含物品）- 从以下选项中选择，如果识别不到则填null：earrings, necklaces, bracelets, rings

18. jewelrySetting（珠宝镶嵌）- 从以下选项中选择最合适的，如果识别不到则填Pavé：
Pavé, Wrap, Semi-Mount, Bead-Set, Waterfall, Trellis, Solitaire, Channel, Shared Prong, Strung, Cathedral, Flush, Illusion, Hand-Wired, Half Bezel, Bezel, Tension Mount, Twist, V-Prong, Side Stone, Prongs, Invisible, Halo

19. karatsMeasure（克拉数值）- 提取金属纯度数值（如10、14、18等），如果识别不到则填null

20. karatsUnit（克拉单位）- 如果识别不到则填kt
注意：karatsMeasure和karatsUnit必须同时填写或同时为null

21. metalStamp（金属印记）- 从以下选项中选择最合适的，如果识别不到则填925 Sterling：
18k, 925 Sterling and 24k, 925 Sterling and 22k, 900 Platinum, 14k, 24k, 925 Sterling, 925 Sterling and 14k, 999 Silver, 950 Platinum, 22k, 950 Palladium, 10k, 20k, 925 Sterling and 18k, No Stamp

22. numberOfGemstones（宝石数量）- 提取产品包含的宝石数量，如果识别不到则填1

23. numberOfPearls（珍珠数量）- 提取产品包含的珍珠数量，如果识别不到则填0

24. numberOfProgs（爪数量）- 提取产品包含的爪数量，如果识别不到则填3

25. pattern（图案）- 提取产品设计主题，如果识别不到则填Unique Design

26. shape（形状）- 提取产品形状（如十字架、爱心、四叶草等），如果识别不到则填空

27. theme（主题）- 提取产品含义或主导思想（如海滩、宗教、爱等），如果识别不到则填Unique

严格按照JSON格式返回：
{
  ""color"":""选择的颜色"",
  ""colorCategory"":""选择的颜色类别"",
  ""gemstoneType"":""选择的宝石类型"",
  ""material"":""选择的材质"",
  ""metalType"":""选择的金属类型"",
  ""netContentMeasure"":数值,
  ""netContentUnit"":""单位"",
  ""assembledProductDepthMeasure"":数值或null,
  ""assembledProductDepthUnit"":""单位""或null,
  ""assembledProductHeightMeasure"":数值或null,
  ""assembledProductHeightUnit"":""单位""或null,
  ""assembledProductWeightMeasure"":数值或null,
  ""assembledProductWeightUnit"":""单位""或null,
  ""assembledProductWidthMeasure"":数值或null,
  ""assembledProductWidthUnit"":""单位""或null,
  ""birthstoneMonth"":""选择的生辰石月份"",
  ""itemsIncluded"":""包含的物品类型"",
  ""jewelrySetting"":""选择的珠宝镶嵌"",
  ""karatsMeasure"":数值或null,
  ""karatsUnit"":""单位""或null,
  ""metalStamp"":""选择的金属印记"",
  ""numberOfGemstones"":数值,
  ""numberOfPearls"":数值,
  ""numberOfProgs"":数值,
  ""pattern"":""图案描述"",
  ""shape"":""形状描述"",
  ""theme"":""主题描述""
}

你只需要将json返回给我，不需要说其他多余的文字，不需要有多余的提示，不需要有多余的表情符号。注意：所有填写或选择的属性值都必须使用英文，不要使用中文或其他语言";
    }
}
