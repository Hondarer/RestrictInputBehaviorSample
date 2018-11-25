using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestrictInputBehaviorSample
{
    /// <summary>
    /// <see cref="DependencyObject"/> に、入力可能な文字に制限を加えるビヘイビアを提供します。
    /// </summary>
    /// <remarks>
    /// <para><see cref="RestrictInputBehavior"/> を使うと、<c>PasswordBox</c> などで禁則文字がある場合に、入力制限を行うことができます。</para>
    /// <para><c>AllowCharacters</c> 添付プロパティに受け入れ可能な文字からなる文字列を設定することにより、
    /// 入力可能な文字に制限を加えることができます。</para>
    /// <para><c>IgnoreCase</c> 添付プロパティにより、大文字小文字を区別するかどうかを指定することができます。</para>
    /// <para>クリップボードからの文字列の貼り付け時には、入力可能な文字のみ維持され、入力不可能な文字は削除されます。</para>
    /// <para>このビヘイビアが添付された <see cref="DependencyObject"/> は、IME が強制的に無効になります。これは仕様です。</para>
    /// </remarks>
    /// <example>
    /// <code language="XAML">
    /// <![CDATA[
    /// <!-- TextBox の例 -->
    /// <TextBox RestrictInputBehavior.AllowCharacters="123abc" />
    /// <TextBox RestrictInputBehavior.AllowCharacters="123abc" RestrictInputBehavior.IgnoreCase="True" />
    /// <!-- PasswordBox の例 -->
    /// <PasswordBox RestrictInputBehavior.AllowCharacters="123abc" />
    /// <PasswordBox RestrictInputBehavior.AllowCharacters="123abc" RestrictInputBehavior.IgnoreCase="True" />]]>
    /// </code>
    /// </example>
    public class RestrictInputBehavior
    {
        #region 添付プロパティ

        /// <summary>
        /// <c>AllowCharacters</c> 添付プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// 許容される入力文字からなる文字列を取得または設定します。
        /// </summary>
        /// <value>
        /// <para>許容される入力文字からなる文字列。</para>
        /// <para><c>null</c> または空文字の場合、入力制限を行いません。既定値は <c>null</c> です。</para>
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty AllowCharactersProperty =
        DependencyProperty.RegisterAttached("AllowCharacters", typeof(string), typeof(RestrictInputBehavior), new UIPropertyMetadata(null, AllowCharactersChanged));

        /// <summary>
        /// <c>AllowCharacters</c> 添付プロパティを取得します。
        /// </summary>
        /// <param name="obj">対象の <see cref="DependencyObject"/>。</param>
        /// <returns><c>AllowCharacters</c> 添付プロパティの値。</returns>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static string GetAllowCharacters(DependencyObject obj)
        {
            return (string)obj.GetValue(AllowCharactersProperty);
        }

        /// <summary>
        /// <c>AllowCharacters</c> 添付プロパティを設定または取得します。
        /// </summary>
        /// <param name="obj">対象の <see cref="DependencyObject"/>。</param>
        /// <param name="value">設定する値。</param>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static void SetAllowCharacters(DependencyObject obj, string value)
        {
            obj.SetValue(AllowCharactersProperty, value);
        }

        /// <summary>
        /// <c>IgnoreCase</c> 添付プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// 許容される入力文字の大文字小文字の差異を無視するかどうかを取得または設定します。
        /// </summary>
        /// <value><c>true</c> の場合は大文字小文字を区別しません。<c>false</c> の場合は大文字小文字を区別します。既定値は <c>false</c> です。</value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IgnoreCaseProperty =
            DependencyProperty.RegisterAttached("IgnoreCase", typeof(bool), typeof(RestrictInputBehavior), new UIPropertyMetadata(false));

        /// <summary>
        /// <c>IgnoreCase</c> 添付プロパティを取得します。
        /// </summary>
        /// <param name="obj">対象の <see cref="DependencyObject"/>。</param>
        /// <returns><c>IgnoreCase</c> 添付プロパティの値。</returns>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static bool GetIgnoreCase(DependencyObject obj)
        {
            return (bool)obj.GetValue(IgnoreCaseProperty);
        }

        /// <summary>
        /// <c>IgnoreCase</c> 添付プロパティを設定します。
        /// </summary>
        /// <param name="obj">対象の <see cref="DependencyObject"/>。</param>
        /// <param name="value">設定する値。</param>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static void SetIgnoreCase(DependencyObject obj, bool value)
        {
            obj.SetValue(IgnoreCaseProperty, value);
        }

        #endregion

        /// <summary>
        /// <c>AllowCharacters</c> 添付プロパティが更新されたイベントを処理するメソッドを表します。
        /// </summary>
        /// <param name="sender">対象の <see cref="DependencyObject"/>。</param>
        /// <param name="e">イベントのデータ。</param>
        private static void AllowCharactersChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = sender as UIElement;
            if (element == null)
            {
                return;
            }

            // イベントを登録・削除する
            element.PreviewTextInput -= UIElement_PreviewTextInput;
            element.PreviewKeyDown -= UIElement_PreviewKeyDown;
            DataObject.RemovePastingHandler(element, UIElement_PastingEventHandler);

            if (string.IsNullOrEmpty(e.NewValue as string) != true)
            {
                element.PreviewTextInput += UIElement_PreviewTextInput;
                element.PreviewKeyDown += UIElement_PreviewKeyDown;
                DataObject.AddPastingHandler(element, UIElement_PastingEventHandler);

                // IME を無効にする
                // IME を許容しつつ、入力文字列の制限を行うのは非常に複雑な処理が必要になる。
                // (PreviewTextInput イベントが発生せず、直接コントロール内部の文字列が変更されるため)
                InputMethod.SetIsInputMethodEnabled(element, false);

                // TextBox の場合、
                // デフォルトの ContextMenu では IME を無効にしてもメニューから再変換ができてしまうので、ContextMenu を変更する
                // ContextMenu プロパティが null でない場合は、スタイルや XAML で何らかのメニューが設定されているので、本処理を行わない。
                if ((element is TextBox) && ((element as TextBox).ContextMenu == null))
                {
                    List<ICommand> menus = new List<ICommand>
                    {
                        ApplicationCommands.Cut,
                        ApplicationCommands.Copy,
                        ApplicationCommands.Paste
                    };

                    ContextMenu menu = new ContextMenu
                    {
                        ItemsSource = menus
                    };

                    (element as TextBox).ContextMenu = menu;
                }
            }
        }

        /// <summary>
        /// 許容された文字のみからなるように、入力文字列をフィルターします。
        /// </summary>
        /// <param name="obj">対象の <see cref="DependencyObject"/>。</param>
        /// <param name="text">入力文字列。</param>
        /// <returns>フィルターされた文字列。</returns>
        private static string ValidText(object obj, string text)
        {
            StringBuilder sb = new StringBuilder();
            string allowCharacters = GetAllowCharacters(obj as DependencyObject);

            // 大文字小文字を区別しない場合は、許容文字列に両方を含んでおく
            if (GetIgnoreCase(obj as DependencyObject) == true)
            {
                allowCharacters = string.Concat(allowCharacters.ToLower(), allowCharacters.ToUpper());
            }

            foreach (char c in text.ToCharArray())
            {
                // 入力文字列が許容文字列に含まれていたら
                if (allowCharacters.Contains(c) == true)
                {
                    // 結果に文字列を追加する
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 対象の <see cref="DependencyObject"/> がテキストを取得したイベントを処理するメソッドを表します。
        /// </summary>
        /// <param name="sender">対象の <see cref="DependencyObject"/>。</param>
        /// <param name="e">イベントのデータ。</param>
        private static void UIElement_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 入力しようとしている文字列をフィルターし、
            // 許容されない文字列の場合は、イベントを破棄する
            if (string.IsNullOrEmpty(ValidText(sender, e.Text)) == true)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 対象の <see cref="DependencyObject"/> にフォーカスがある状態でキーが押されたときに発生したイベントを処理するメソッドを表します。
        /// </summary>
        /// <param name="sender">対象の <see cref="DependencyObject"/>。</param>
        /// <param name="e">イベントのデータ。</param>
        private static void UIElement_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // PreviewTextInput では半角スペースを検知できないので、PreviewKeyDown で検知
            if (e.Key == Key.Space)
            {
                if (string.IsNullOrEmpty(ValidText(sender, " ")) == true)
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// <c>DataObject.Pasting</c> 添付イベントを処理するメソッドを表します。
        /// </summary>
        /// <param name="sender">対象の <see cref="DependencyObject"/>。</param>
        /// <param name="e">イベントのデータ。</param>
        private static void UIElement_PastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            // 貼り付けようとしている文字列を取得
            string clipboard = e.DataObject.GetData(typeof(string)) as string;

            // 許容された文字列のみになるよう、フィルターする
            clipboard = ValidText(sender, clipboard);

            // フィルタの結果、貼り付けるべき文字列がなくなったら、コマンド自体をキャンセルする
            if (string.IsNullOrEmpty(clipboard) == true)
            {
                e.CancelCommand();
                e.Handled = true;
            }

            // 貼り付けるべきデータをフィルター済みの文字列に差し替える
            // DataObject は Freeze されているので、新たに DataObject を生成してセットする
            DataObject d = new DataObject();
            d.SetData(DataFormats.Text, clipboard);
            e.DataObject = d;

            return;
        }
    }
}
