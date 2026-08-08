import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-url-stats-page',
  imports: [],
  template: `<p>url-stats-page works!</p>`,
  styleUrl: './url-stats-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UrlStatsPage {}
