﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CCWin.SkinControl;
using ESBasic.ObjectManagement.Managers;
using ESBasic;
using JustLib.UnitViews;
using System.Threading;
using JustLib.Properties;


namespace JustLib.UnitViews
{
    public partial class UserListBox : UserControl
    {
        private IUserInformationForm userInformationForm;//悬浮至头像时   
        private ChatListSubItem myselfChatListSubItem;
        private IUser currentUser;
       
        public UserListBox()
        {
            InitializeComponent();

            
        }


        

        /// <summary>
        /// 添加分组的菜单被点击。参数:组名称
        /// </summary>
        public event CbGeneric AddCatalogClicked;
        /// <summary>
        /// 修改组名的菜单被点击。参数:组名称
        /// </summary>
        public event CbGeneric<string> ChangeCatalogNameClicked;
        public event CbGeneric<ChatListSubItem> UserDoubleClicked;
        public event CbGeneric<IUser> ChatRecordClicked;
        public event CbGeneric<IUser> RemoveUserClicked;

       // public event CbGeneric<ChatListSubItem> 
        
        /// <summary>
        /// 当修改分组名称时，触发此事件。参数：oldName - newName - isMerge
        /// </summary>
        public event CbGeneric<string, string, bool> CatalogNameChanged;
        /// <summary>
        /// 当增加一个分组时，触发此事件。参数：CatelogName
        /// </summary>
        public event CbGeneric<string> CatalogAdded;
        /// <summary>
        /// 当删除一个分组时，触发此事件。参数：CatelogName
        /// </summary>
        public event CbGeneric<string> CatalogRemoved;
        /// <summary>
        /// 当将好友转移到另一组时，触发此事件。参数： FriendID - oldCatalog - newCatalogName
        /// </summary>
        public event CbGeneric<string, string, string> FriendCatalogMoved;

        public ChatListItemIcon IconSizeMode
        {
            get
            {
                return this.chatListBox.IconSizeMode;
            }
            set
            {
                this.chatListBox.IconSizeMode = value;
                this.chatListBox.Invalidate();
            }
        }

        private IHeadImageGetter resourceGetter;
        public void Initialize(IUser current ,IHeadImageGetter getter ,IUserInformationForm form)
        {
            this.resourceGetter = getter;
            this.currentUser = current;
            this.userInformationForm = form;
            if (this.userInformationForm != null)
            {
                ((Form)this.userInformationForm).Visible = false;
            }

            this.AssureCatalog(this.currentUser.DefaultFriendCatalog);
            foreach (string catalog in this.currentUser.GetFriendCatalogList())
            {
                this.AssureCatalog(catalog);
            }
        }

        private void AssureCatalog(string catalog)
        {
            if (!this.catelogManager.Contains(catalog))
            {
                ChatListItem item = new ChatListItem(catalog);
                this.catelogManager.Add(catalog, item);
                this.chatListBox.Items.Add(item);
             
               // this.chatListBox.Items.Sort();
              
            }
        }

        #region GetCatelogChatListItem(添加组)
        private ObjectManager<string, ChatListItem> catelogManager = new ObjectManager<string, ChatListItem>();
        public ChatListItem GetCatelogChatListItem(IUser user,string groupName)
        {
                       
                string catelog = groupName;
                this.AssureCatalog(catelog);
                return this.catelogManager.Get(catelog);

        }
        #endregion

      
        public void AddUser(GGUser friend,string groupName)
        {
            //ChatListSubItem[] items = this.chatListBox.GetSubItemsById(friend.ID);
            //if (items != null && items.Length > 0)
            //{
            //    return;
            //}
            
            if (friend == null)
            {
                this.GetCatelogChatListItem(null, groupName);
            }
            else
            {
                if (groupName == "")
                {
                    //Resource.Head_portrait02_index_32

              
                    System.IO.MemoryStream ms = new System.IO.MemoryStream(friend.HeadImageData);
                    System.Drawing.Image img = System.Drawing.Image.FromStream(ms);

                    ChatListSubItem subItem1 = new ChatListSubItem(friend.ID, "", friend.Name, friend.UserCompany, this.ConvertUserStatus(friend.UserStatus),img);
                    subItem1.Tag = friend;
                    
                    groupName = "我的好友";
                    this.GetCatelogChatListItem(friend, groupName).SubItems.AddAccordingToStatus(subItem1);
                    this.myselfChatListSubItem = subItem1;

                    subItem1.OwnerListItem.SubItems.Sort();

                }
                else
                {

                    ChatListSubItem subItem1 = new ChatListSubItem(friend.ID, "", friend.Name, friend.UserCompany, this.ConvertUserStatus(friend.UserStatus), Resource.Head_portrait02_index_32);
                    subItem1.Tag = friend;
                    this.GetCatelogChatListItem(friend, "我的好友");
                    this.GetCatelogChatListItem(friend, groupName).SubItems.AddAccordingToStatus(subItem1);
                    this.myselfChatListSubItem = subItem1;

                    subItem1.OwnerListItem.SubItems.Sort();
                    
                }
               

            }


            //ChatListSubItem subItem = new ChatListSubItem(friend.ID, "", friend.Name, friend.Signature, this.ConvertUserStatus(friend.UserStatus), Resource.Head_portrait02_index_32);
          
            //subItem.Tag = friend;

          
            //    if (groupName == "")
            //    {
            //        groupName = "我的好友";

            //        this.GetCatelogChatListItem(friend, groupName).SubItems.AddAccordingToStatus(subItem);

            //    }
            //    else
            //    {


            //        this.GetCatelogChatListItem(friend, "我的好友");
            //        this.GetCatelogChatListItem(friend, groupName).SubItems.AddAccordingToStatus(subItem);


            //    }
               
          

           
         
        }


        public void SetUserHead(string JID, byte[] ImageBytes)
        {
            foreach (ChatListSubItem subItem in this.chatListBox.Items[0].SubItems)
            {
                if (subItem.ID == JID)
                {
                    System.IO.MemoryStream ms = new System.IO.MemoryStream(ImageBytes);
                    System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                    subItem.HeadImage = img;
                    this.chatListBox.Invoke(new Action(() =>
                    {
                        this.chatListBox.Invalidate(subItem.HeadRect);
                    }));
                }
            }
        }

        public void ExpandRoot()
        {
            this.chatListBox.Items[0].IsOpen = true;
        }

        public void SetAllUserOffline()
        {
            foreach (ChatListItem item in this.chatListBox.Items)
            {
                foreach (ChatListSubItem sub in item.SubItems)
                {
                    sub.Status = ChatListSubItem.UserStatus.OffLine;
                }                
            }

            this.chatListBox.Invalidate();
        }

        public void SortAllUser()
        {
            foreach (ChatListItem item in this.catelogManager.GetAll())
            {
                if (item.SubItems.Count > 0)
                {
                   item.SubItems.Sort();
                }
            }
        }

        public void RemoveUser(string userID)
        {
            this.chatListBox.RemoveSubItemsById(userID);
            this.chatListBox.Invalidate();           
        }

        public bool ContainsUser(string userID)
        {
            ChatListSubItem[] items = this.chatListBox.GetSubItemsById(userID);
            return (items != null && items.Length > 0);
        }

        public void SetTwinkleState(string userID, bool twinkle)
        {
            ChatListSubItem[] items = this.chatListBox.GetSubItemsById(userID);
            if (items == null || items.Length == 0)
            {
                return;
            }
            items[0].IsTwinkle = twinkle;
        }

        public void UserStatusChanged(GGUser user,string groupName)
        {           
            ChatListSubItem[] items = this.chatListBox.GetSubItemsById(user.ID);
            if (items == null || items.Length == 0)
            {
                return;
            }

            //System.IO.MemoryStream ms = new System.IO.MemoryStream(user.HeadImageData);
            //System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            //items[0].HeadImage = img;
            items[0].Status = this.ConvertUserStatus(user.UserStatus);

            if (groupName == "")
            {
                groupName = "我的好友";
            }
            ChatListItem item = this.catelogManager.Get(groupName);
            if (item != null)
            {
                item.SubItems.Sort();
            }
            this.chatListBox.Invalidate();           
        }


        
        public List<ChatListSubItem> SearchChatListSubItem(string idOrName)
        {
            ChatListSubItem[] items = this.chatListBox.GetSubItemsByText(idOrName);
            List<ChatListSubItem> list = new List<ChatListSubItem>();
            if (items != null)
            {
                foreach (ChatListSubItem item in items)
                {
                    if (item.ID != this.currentUser.ID)
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public void UserInfoChanged(string userID, IUser newUser)
        {          
            ChatListSubItem[] items = this.chatListBox.GetSubItemsById(userID);
            if (items.Length > 0)
            {
                items[0].Tag = newUser;
            }
           // IMCore.GGUser user = ((IMCore.GGUser)(items[0].Tag));

            //IUser origin = (IUser)items[0].Tag["jid"];



            //if (items != null && items.Length > 0) 
            //{
            //    this.RemoveUser(user.ID);
            //    this.AddUser(user, groupName);
            //}

            //if (items != null && items.Length > 0) //有可能部门发生了变化
            //{
            //    IUser origin = (IUser)items[0].Tag;
            //    ChatListItem ownerItem = this.GetCatelogChatListItem(origin, groupName);
            //    ownerItem.SubItems.Remove(items[0]);
            //    this.AddUser(user, groupName); //有可能是新添加的好友
            //}            
        }



        private void toolStripMenuItem51_Click(object sender, EventArgs e)
        {
            ChatListSubItem item = this.chatListBox.SelectSubItem;
            //IUser friend = (IUser)item.Tag;
            item.IsTwinkle = false;

            //if (friend.ID == this.currentUser.ID)
            //{
            //    return;
            //}

            if (this.UserDoubleClicked != null)
            {
                this.UserDoubleClicked(item);
            }
        }

        private void 消息记录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (this.currentUser.UserStatus == UserStatus.OffLine)
            //{
            //    return;
            //}

            IUser friend = (IUser)this.chatListBox.SelectSubItem.Tag;
            //if (friend.ID == this.currentUser.ID)
            //{
            //    return;
            //}

            if (this.ChatRecordClicked != null)
            {
                this.ChatRecordClicked(friend);
            }
        }

        private void 删除好友ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (this.currentUser.UserStatus == UserStatus.OffLine)
            //{
            //    return;
            //}

            IUser friend = (IUser)this.chatListBox.SelectSubItem.Tag;
            //if (friend.ID == this.currentUser.ID)
            //{
            //    return;
            //}

            if (this.RemoveUserClicked != null)
            {
                this.RemoveUserClicked(friend);
            }
        }

        private void chatListBox_DoubleClickSubItem(object sender, ChatListEventArgs e)
        {
            ChatListSubItem item = e.SelectSubItem;
            //IUser friend = (IUser)item.Tag;
            item.IsTwinkle = false;
            
            
            //if (friend.ID == this.currentUser.ID)
            //{
            //    return;
            //}

            if (this.UserDoubleClicked != null)
            {
                this.UserDoubleClicked(item);
            }
        }

        #region 显示用户资料
        private bool firstShow = false;
        private void chatShow_MouseEnterHead(object sender, ChatListEventArgs e)
        {
            if (this.userInformationForm == null)
            {
                return;
            }

            ChatListSubItem item = e.MouseOnSubItem;
            if (item == null)
            {
                item = e.SelectSubItem;
            }

            Point loc = this.PointToScreen(this.Location);

            //int top = this.Top + this.chatListBox.Top + (item.HeadRect.Y - this.chatListBox.chatVScroll.Value);
            //int left = this.Left - 279 - 5;
            int top = loc.Y + (item.HeadRect.Y - this.chatListBox.chatVScroll.Value) - this.Location.Y;
            int left = loc.X - 279 - 5;
            //int ph = Screen.GetWorkingArea(this).Height;

            //if (top + 181 > ph)
            //{
            //    top = ph - 181 - 5;
            //}

            if (left < 0)
            {
                left = this.Right + 5;
            }

            IUser user = (IUser)item.Tag;
            Form form = (Form)this.userInformationForm;            
            this.userInformationForm.SetUser(user);           
            form.Location = new Point(left, top);
            if (!this.firstShow)
            {
                form.Show();
            }
            else
            {
                this.firstShow = true;
            }
            form.Location = new Point(left, top);
        }

        private void chatShow_MouseLeaveHead(object sender, ChatListEventArgs e)
        {
            if (this.userInformationForm == null)
            {
                return;
            }

            Thread.Sleep(100);
            Form form = (Form)this.userInformationForm;
            if (!form.Bounds.Contains(Cursor.Position))
            {
                form.Hide();
            }
        }
        #endregion

        private void chatListBox_DragSubItemDrop(object sender, DragListEventArgs e)
        {
            if (this.FriendCatalogMoved != null)
            {
                IUser user = (IUser) e.QSubItem.Tag ;
                this.FriendCatalogMoved(user.ID, e.QSubItem.OwnerListItem.Text, e.HSubItem.OwnerListItem.Text);
            }
        }

        public void ChangeCatelogName(string oldName, string newName)
        {
            //if (this.currentUser.UserStatus == UserStatus.OffLine)
            //{
            //    return;
            //}

            this.catelogManager.Remove(oldName);
            ChatListItem existedItem = null;
            foreach (ChatListItem item in this.chatListBox.Items)
            {
                if (item.Text == newName)
                {
                    existedItem = item;
                    break;
                }
            }
            if (existedItem != null)
            {
                foreach (ChatListSubItem sub in this.chatListBox.SelectItem.SubItems)
                {
                    sub.OwnerListItem = existedItem;
                    existedItem.SubItems.Add(sub);
                }
                existedItem.SubItems.Sort();
                this.chatListBox.Items.Remove(this.chatListBox.SelectItem);
                existedItem.IsOpen = true;
                if (this.CatalogNameChanged != null)
                {
                    this.CatalogNameChanged(oldName, newName, true);
                }
                return;
            }

            this.catelogManager.Add(newName, this.chatListBox.SelectItem);
            this.chatListBox.SelectItem.Text = newName;
            if (this.CatalogNameChanged != null)
            {
                this.CatalogNameChanged(oldName, newName, false);
            }
        }

        private void 修改名称ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (this.currentUser.UserStatus == UserStatus.OffLine)
            //{
            //    return;
            //}

            string oldName = this.chatListBox.SelectItem.Text;
            if (this.ChangeCatalogNameClicked != null)
            {
                this.ChangeCatalogNameClicked(oldName);
            }
        }

        public void AddCatalog(string catelogName)
        {
            //if (this.currentUser.UserStatus == UserStatus.OffLine)
            //{
            //    return;
            //}

            foreach (ChatListItem item in this.chatListBox.Items)
            {
                if (item.Text == catelogName)
                {
                    MessageBox.Show("已经存在同名的分组！");
                    return;
                }
            }

            ChatListItem newItem = new ChatListItem(catelogName);
            this.catelogManager.Add(catelogName, newItem);
            this.chatListBox.Items.Add(newItem);
            //this.chatListBox.Items.Sort();

            if (this.CatalogAdded != null)
            {
                this.CatalogAdded(catelogName);
            }
        }

        private void 添加分组ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (this.currentUser.UserStatus == UserStatus.OffLine)
            //{
            //    return;
            //}

            if (this.AddCatalogClicked != null)
            {
                this.AddCatalogClicked();
            }
        }

        private void 删除分组ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (this.currentUser.UserStatus == UserStatus.OffLine)
            //{
            //    return;
            //}

            string name = this.chatListBox.SelectItem.Text;
            if (this.currentUser.DefaultFriendCatalog == name)
            {
                MessageBox.Show(string.Format("分组 [{0}] 是默认分组，不能删除！", name));
                return;
            }

            if (this.chatListBox.SelectItem.SubItems.Count > 0)
            {
                MessageBox.Show(string.Format("分组 [{0}] 不为空，不能删除！" ,name));
                return;
            }

            if (!ESBasic.Helpers.WindowsHelper.ShowQuery(string.Format("您确定要删除分组 [{0}] 吗？", name)))
            {
                return;
            }

            this.chatListBox.Items.Remove(this.chatListBox.SelectItem);
            this.catelogManager.Remove(name);
            if (this.CatalogRemoved != null)
            {
                this.CatalogRemoved(name);
            }
        }

        #region ConvertUserStatus
        private ChatListSubItem.UserStatus ConvertUserStatus(UserStatus status)
        {
            if (status == UserStatus.Hide)
            {
                return ChatListSubItem.UserStatus.OffLine;
            }

            return (ChatListSubItem.UserStatus)((int)status);
        }
        #endregion
    }
}
