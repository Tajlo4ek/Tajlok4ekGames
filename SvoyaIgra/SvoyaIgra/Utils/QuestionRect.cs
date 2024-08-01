using System.Drawing;

namespace SvoyaIgra.Utils
{
    public class ChoiceRect
    {
        private Rectangle rect;

        public int QuestionId { get; private set; }

        public int ThemeId { get; private set; }

        public ChoiceRect(Rectangle rect, int themeId, int questionId)
        {
            this.rect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);

            this.QuestionId = questionId;
            this.ThemeId = themeId;
        }

        public ChoiceRect(Rectangle rect, int themeId) : this(rect, themeId, -1)
        {

        }

        public bool IsInside(int x, int y)
        {
            return rect.Contains(x, y);
        }

        public Rectangle GetRect()
        {
            return rect;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return ThemeId ^ QuestionId;
        }

    }
}
