﻿using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace TW03
{
  class Program
  {
    static void Main(string[] args)
    {
      // 明細のテストデータ
      // 画面から入力された商品IDと個数、および、
      // データベースから取得してきた税別単価と消費税率を作業用のリストに格納したと想定
      var 明細リスト = new List<明細>
                      {
                        new 明細{ 商品ID="商品AAA", 個数=5, 税別単価=10, 消費税率=0.08m},
                        new 明細{ 商品ID="商品BBB", 個数=1, 税別単価=50, 消費税率=0.08m},
                        new 明細{ 商品ID="商品CCC", 個数=10, 税別単価=42, 消費税率=0.10m},
                        //new 明細{ 商品ID="商品DDD", 個数=1, 税別単価=31, 消費税率=0.00m},
                      };

      // 明細行ごとに、税込単価と金額を算出
      foreach (var 明細 in 明細リスト)
      {
        // 単品ごとの消費税は、税別単価に消費税率を掛けて四捨五入
        明細.消費税単価 = Math.Round(明細.税別単価 * 明細.消費税率);
        // ※Math.Round は、IEEE 754 方式の四捨五入（銀行丸め）
        // 単品の税込単価は、税別単価に消費税を足す
        明細.税込単価 = 明細.税別単価 + 明細.消費税単価;
        // 明細行の金額は、個数×税込単価
        明細.金額 = 明細.個数 * 明細.税込単価;
      }

      // 明細リストに含まれる消費税率を数え上げる（複数税率対応）
      // ※この例では消費税率リストに8%と10%が入る
      var 消費税率リスト = 明細リスト.Select(明細 => 明細.消費税率)
                                     .Distinct().OrderBy(消費税率 => 消費税率);

      // 消費税率ごとに明細行を集計
      // 消費税率をキーとするハッシュテーブルを用意する
      var 税率別小計金額 = new Dictionary<decimal, decimal>();
      var 税率別小計消費税 = new Dictionary<decimal, decimal>();
      foreach (decimal 税率 in 消費税率リスト)
      {
        // 消費税率ごとに集計し、ハッシュテーブルに格納する

        // 消費税率が一致する明細行を抜き出す
        IEnumerable<明細> 該当明細群 = 明細リスト.Where(明細 => 明細.消費税率 == 税率);

        // 金額を集計
        decimal 小計金額 = 該当明細群.Sum(明細 => 明細.金額);
        税率別小計金額.Add(税率, 小計金額);

        // 消費税額を集計
        decimal 小計消費税 = 該当明細群.Sum(明細 => 明細.個数 * 明細.消費税単価);
        税率別小計消費税.Add(税率, 小計消費税);
      }

      // 消費税率ごとのデータを全て集計する
      decimal 総合計 = 税率別小計金額.Sum(kv => kv.Value);
      decimal 総合計消費税 = 税率別小計消費税.Sum(kv => kv.Value);

      // 表示テスト
      // 明細
      foreach (var 明細 in 明細リスト)
        WriteLine($"{明細.商品ID} {明細.消費税率:P0} {明細.税込単価}円x{明細.個数}={明細.金額}円");
      // 合計
      WriteLine($"合計 {総合計}円 (内消費税 {総合計消費税}円)");
      foreach (decimal 税率 in 消費税率リスト)
        WriteLine($"{税率:P0} {税率別小計金額[税率]}円 (内消費税 {税率別小計消費税[税率]}円)");

      // 出力：
      // 商品AAA 8% 11円x5=55円
      // 商品BBB 8% 54円x1=54円
      // 商品CCC 10% 46円x10=460円
      // 合計 569円 (内消費税 49円)
      // 8% 109円 (内消費税 9円)
      // 10% 460円 (内消費税 40円)

      WriteLine();
      WriteLine("正しくは、");
      税率別小計金額.Clear();
      税率別小計消費税.Clear();
      foreach (decimal 税率 in 消費税率リスト)
      {
        // 消費税率が一致する明細行を抜き出す
        IEnumerable<明細> 該当明細群 = 明細リスト.Where(明細 => 明細.消費税率 == 税率);

        // 金額を集計
        decimal 小計金額 = 該当明細群.Sum(明細 => 明細.金額);
        税率別小計金額.Add(税率, 小計金額);

        // 消費税額を集計
        //decimal 小計消費税 = 該当明細群.Sum(明細 => 明細.個数 * 明細.消費税単価);
        // ↓正しくは、消費税率別の小計金額と消費税率から計算する
        decimal 小計消費税 = Math.Round(小計金額 * 税率 / (1.00m + 税率));
        税率別小計消費税.Add(税率, 小計消費税);
      }
      decimal 正しい消費税額 = 税率別小計消費税.Sum(kv => kv.Value);
      WriteLine($"合計 {総合計}円 (内消費税 {正しい消費税額}円)");
      foreach (decimal 税率 in 消費税率リスト)
        WriteLine($"{税率:P0} {税率別小計金額[税率]}円 (内消費税 {税率別小計消費税[税率]}円)");

      // 出力：
      // 正しくは、
      // 合計 569円 (内消費税 50円)
      // 8% 109円 (内消費税 8円)
      // 10% 460円 (内消費税 42円)

#if DEBUG
      ReadKey();
#endif
    }
  }
}
