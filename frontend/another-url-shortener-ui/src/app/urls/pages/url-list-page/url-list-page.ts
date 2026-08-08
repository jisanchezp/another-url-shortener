import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-url-list-page',
  imports: [],
  template: `<p>url-list-page works!</p>`,
  styleUrl: './url-list-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UrlListPage {}
