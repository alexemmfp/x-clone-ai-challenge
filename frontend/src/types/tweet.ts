export interface Tweet {
  id: string
  authorId: string
  authorUsername: string
  text: string
  parentId: string | null
  imageUrl: string | null
  createdAt: string
  likeCount: number
  likedByViewer: boolean
}

export interface CreateTweetRequest {
  text: string
  parentId?: string
}
