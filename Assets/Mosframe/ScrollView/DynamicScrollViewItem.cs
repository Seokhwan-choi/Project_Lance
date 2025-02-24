/**
 * IDynamicScrollViewItem.cs
 * 
 * @author mosframe / https://github.com/mosframe
 * 
 */

namespace Mosframe {

    /// <summary>
    /// DynamicScrollView Item interface
    /// </summary>
    public interface IDynamicScrollViewItem {
        void Init();
	    void OnUpdateItem( int index );
    }
}
